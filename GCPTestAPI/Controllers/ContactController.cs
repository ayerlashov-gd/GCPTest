using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GCPTestAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContactController : ControllerBase
    {
        public ContactController(IConfiguration configuration)
        {
            Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            ContactContext = new ContactContext(configuration);
        }

        public ContactContext ContactContext { get; }
        public IConfiguration Configuration { get; }

        [HttpGet]
        public IEnumerable<Contact> Get()
        {
            return ContactContext.Contacts;
        }

        [HttpGet("{id}")]
        public Contact Get(Guid id)
        {
            return ContactContext.Contacts
                .SingleOrDefault(c => c.Id == id);
        }

        [HttpPost]
        public Guid Post([FromBody] WriteContact writeContact)
        {
            var contact = new Contact
            {
                Email = writeContact.Email,
                Address = writeContact.Address,
                Name = writeContact.Name,
                PhoneNumber = writeContact.PhoneNumber
            };

            ContactContext.Add(contact);
            
            ContactContext.SaveChanges();

            return contact.Id;
        }


        [HttpPut]
        public void Put([FromBody] Contact contact)
        {
            ContactContext.Update(contact);
            ContactContext.SaveChanges();
        }

        [HttpDelete("{id}")]
        public void Delete(Guid id)
        {
            ContactContext.Remove(new Contact { Id = id });
            ContactContext.SaveChanges();
        }
    }
}
