using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace GCPTestAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FirestoreController : ControllerBase
    {
        private const string ProjectId = "gpc-dotnetcore-viability-test";

        private static FirestoreDb DB { get; } = FirestoreDb.Create(ProjectId);

        [HttpGet("all")]
        public async Task<IActionResult> GetAll()
        {
            var docs = new Dictionary<string, DocumentSnapshotDTO>();
            var collections = new Dictionary<string, CollectionReferenceDTO>();

            var currentCollections = await DB.ListRootCollectionsAsync().ToListAsync();

            while (currentCollections.Count > 0)
            {
                currentCollections.ForEach(col => collections[col.Path] = Convert(col));

                var currentDocs = currentCollections.SelectMany(
                    col => col.ListDocumentsAsync()
                    .Where(docRef => !docs.ContainsKey(docRef.Path))
                    .Select(docRef => (Reference: docRef, Snapshot: docRef.GetSnapshotAsync(), Collections: docRef.ListCollectionsAsync()))
                    .ToEnumerable()
                    )
                    .ToList();

                if (currentDocs.Count != 0)
                {
                    currentCollections = new List<CollectionReference>();

                    foreach (var doc in currentDocs)
                    {
                        docs[doc.Reference.Path] = Convert(doc.Reference.Id, await doc.Snapshot);
                        currentCollections.AddRange(await doc.Collections.Where(col => !collections.ContainsKey(col.Path)).ToListAsync());
                    }

                    currentCollections = currentCollections
                        .GroupBy(c => c.Path, (path, set) => set.First())
                        .ToList();
                }
            }

            return Ok(new
            {
                Documents = docs.Values,
                Collections = collections.Values
            });
        }

        [HttpGet("collection/root")]
        public async Task<IActionResult> GetRootCollections()
        {
            var result = await DB.ListRootCollectionsAsync().ToListAsync();

            var mappedResult = result.Select(Convert);

            return Ok(mappedResult);
        }

        [HttpGet("collection/{path}")]
        public async Task<IActionResult> GetCollection(string path)
        {
            path = path.DecodePath();

            var result = await DB.Collection(path)
                .ListDocumentsAsync()
                .Select(async docRef => Convert(docRef.Id, await docRef.GetSnapshotAsync()))
                .ToListAsync();

            return Ok(result);
        }

        [HttpPost("document/{path}")]
        public async Task<IActionResult> PostDocument(string path, [FromBody] JsonElement documentJson)
        {
            path = path.DecodePath();

            var result = await DB.Document(path).SetAsync(Convert(documentJson));

            return Ok(result);
        }

        [HttpGet("document/{path}/collections")]
        public async Task<IActionResult> GetDocumentCollections(string path)
        {
            path = path.DecodePath();

            var result = await DB.Document(path)
                .ListCollectionsAsync()
                .Select(doc => Convert(doc))
                .ToListAsync();

            return Ok(result);
        }

        [HttpGet("document/{path}")]
        public async Task<IActionResult> GetDocument(string path)
        {
            path = path.DecodePath();

            var docRef = DB.Document(path);
            var snapshot = await docRef.GetSnapshotAsync();

            if (!snapshot.Exists)
                return NotFound();

            var result = Convert(docRef.Id, snapshot);

            return Ok(result);
        }

        [HttpDelete("document/{path}")]
        public async Task<IActionResult> Delete(string path)
        {
            path = path.DecodePath();

            var result = await DB.Document(path).DeleteAsync();

            return Ok(result);
        }

        private static CollectionReferenceDTO Convert(CollectionReference doc) =>
            new CollectionReferenceDTO
            {
                Id = doc.Id,
                Path = doc.Path
            };

        private static DocumentSnapshotDTO Convert(string id, DocumentSnapshot snapshot)
        {
            var dictStack = new Stack<Dictionary<string, object>>();
            var listStack = new Stack<List<object>>();

            var result = new DocumentSnapshotDTO
            {
                Id = id,
                CreateTime = snapshot.CreateTime.Convert(),
                UpdateTime = snapshot.UpdateTime.Convert(),
                Path = snapshot.Reference.Path,
                Doc = snapshot.ToDictionary()
            };
                

            dictStack.Push(result.Doc);

            while (dictStack.Count > 0)
            {
                var current = dictStack.Pop();

                foreach (var pair in current.ToList())
                {
                    ProcessElement(pair.Value, res => current[pair.Key] = res);
                }

                while (listStack.Count > 0)
                {
                    var currentList = listStack.Pop();

                    for (int i = 0; i < currentList.Count; i++)
                    {
                        ProcessElement(currentList[i], res => currentList[i] = res);
                    }
                }
            }

            void ProcessElement(object element, Action<object> setter)
            {
                switch (element)
                {
                    case Timestamp ts:
                        setter(ts.ToDateTime());
                        break;
                    case Dictionary<string, object> dict:
                        dictStack.Push(dict);
                        break;
                    case List<object> list:
                        listStack.Push(list);
                        break;
                }
            }

            return result;
        }

        private static object Convert(JsonElement json)
        {
            switch (json.ValueKind)
            {
                case JsonValueKind.Object:
                    return json.EnumerateObject()
                        .ToDictionary(p => p.Name, p => Convert(p.Value));
                case JsonValueKind.Array:
                    return json.EnumerateArray()
                        .Select(e => Convert(e))
                        .ToArray();
                case JsonValueKind.String:
                    if (json.TryGetDateTime(out var dt))
                        return DateTime.SpecifyKind(dt, DateTimeKind.Utc);
                    else if (json.TryGetGuid(out var g))
                        return g;
                    else return json.GetString();
                case JsonValueKind.Number:
                    if (json.TryGetInt64(out var l))
                        return l;
                    else if (json.TryGetDouble(out var d))
                        return d;
                    else if (json.TryGetDecimal(out var m))
                        return m;
                    else return json.GetString();
                case JsonValueKind.True:
                    return true;
                case JsonValueKind.False:
                    return false;
                case JsonValueKind.Undefined:
                case JsonValueKind.Null:
                default:
                    return null;
            }

        }

        public class DocumentSnapshotDTO
        {
            public string Id { get; internal set; }
            public DateTime? CreateTime { get; set; }
            public DateTime? UpdateTime { get; set; }
            public string Path { get; set; }
            public Dictionary<string, object> Doc { get; set; }
        }

        public class CollectionReferenceDTO
        {
            public string Id { get; set; }
            public string Path { get; set; }
        }
    }
}
