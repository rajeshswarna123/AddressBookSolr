using ContactService.DataServices;
using ContactService.Models;
using Services.Automapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Extensions;
using SolrNet;
using CommonServiceLocator;
using System.IO;

namespace ContactService
{
     public class ContactServices
     {

        public class ContctService : IContactService
        {
            ISolrOperations<DataServices.Contact> solr { get; set; }

            public AddressBookDB context { get; set; }

            public ContctService(AddressBookDB _context )
            {
                this.context = _context;
                this.solr = ServiceLocator.Current.GetInstance<ISolrOperations<DataServices.Contact>>();
            }

            #region Contact Service(s)

            public Models.Contact CreateContact(DataServices.Contact contact)
            {
                context.Insert(contact);
                var resp = solr.Add(contact);
                var commitResponse = solr.Commit();
                //var solr = ServiceLocator.Current.GetInstance<ISolrOperations<DataServices.Contact>>();
                //    solr.Delete(SolrQuery.All);
                //    var connection = ServiceLocator.Current.GetInstance<ISolrConnection>();
                //    connection.Post("/update", "");
                //    solr.Commit();
                //    solr.BuildSpellCheckDictionary();
                return contact.MapTo<DataServices.Contact, Models.Contact>();
            }

            public int DeleteContact(int Id)
            {
                context.Execute("Delete From Contact Where Id = @0", Id);
                solr.Delete(Id+"");
                solr.Commit();
                return Id;
            }

            public Models.Contact UpdateContact(DataServices.Contact contact)
            {
                solr.Add(contact);
                solr.Commit();
                context.Update(contact);
                return contact.MapTo<DataServices.Contact, Models.Contact>();
            }

            public Models.Contact GetContact(int Id)
            {
                var sql = PetaPoco.Sql.Builder.Select("*").From("Contact").Where("Id=@0", Id);
                return (context.FirstOrDefault<DataServices.Contact>(sql)).MapTo<DataServices.Contact, Models.Contact>();
            }

            public List<Models.Contact> GetContacts()
            {
                var sql = PetaPoco.Sql.Builder.Select("*").From("Contact");
                return (context.Fetch<DataServices.Contact>(sql)).MapToCollection<DataServices.Contact,Models.Contact>();
            }

            public List<Models.Contact> SearchItem(string input)
            {
                //var query = new SolrQueryByField("collector","*" +input + "*") { Quoted = false };
                var result = solr.Query(new SolrQuery("collector:*"+input+"*"));
                return result.MapToCollection<DataServices.Contact, Models.Contact>();
               
            }
            /*public void indexAllItems()
            {
                foreach (var item in GetContacts())
                {
                    var contact = item.MapTo<Models.Contact, DataServices.Contact>();
                    solr.Add(contact);
                }
                solr.Commit();
            }*/
            #endregion
        }
    }
}
