using LiteDB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using tx_import_acroforms.Models;
using TXTextControl;

namespace tx_import_acroforms.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            using (var db = new LiteDatabase(Server.MapPath("~/App_Data/customers.db")))
            {
                var col = db.GetCollection<Customer>("customers");
                
                var results = col.Query()
                    .ToList();

                return View(results);
            }
        }

        [HttpPost]
        public ActionResult GenerateForm(int Id)
        {
            Customer customer;
            byte[] baCreatedFormDocument;

            // get customer by Id from database
            using (var db = new LiteDatabase(Server.MapPath("~/App_Data/customers.db")))
            {
                var col = db.GetCollection<Customer>("customers");
                
                col.EnsureIndex(x => x.Id);
                customer = col.Query()
                    .Where(x => x.Id == Id)
                    .SingleOrDefault();
            }

            using (TXTextControl.ServerTextControl tx = new TXTextControl.ServerTextControl())
            {
                tx.Create();

                // the form template
                tx.Load(Server.MapPath("~/App_data/form_template.tx"), TXTextControl.StreamType.InternalUnicodeFormat);

                // loop through all form fields of each text part
                foreach (IFormattedText textPart in tx.TextParts)
                {
                    foreach (FormField formField in textPart.FormFields)
                    {
                        // get associated value of property; check for null!
                        var propertyValue = typeof(Customer).GetProperty(formField.Name)?.GetValue(customer);

                        // cast values for specific form field types
                        switch (formField.GetType().ToString())
                        {
                            case "TXTextControl.TextFormField": // accepts strings
                            case "TXTextControl.SelectionFormField":
                                if (propertyValue != null)
                                    ((TextFormField)formField).Text = (string)propertyValue;
                                break;

                            case "TXTextControl.CheckFormField": // accepts bool
                                if (propertyValue != null)
                                    ((CheckFormField)formField).Checked = (bool)propertyValue;
                                break;

                            case "TXTextControl.DateFormField": // accepts Win32 file dates
                                if (propertyValue != null)
                                {
                                    if (Convert.ToDateTime(propertyValue.ToString()) != DateTime.MinValue)
                                        ((DateFormField)formField).Date =
                                            Convert.ToDateTime(propertyValue.ToString());
                                }
                                break;
                        }
                    }
                }

                // export form
                tx.Save(out baCreatedFormDocument, BinaryStreamType.AdobePDF);
            }

            // return the PDF as an attachment
            MemoryStream pdfStream = new MemoryStream();
            pdfStream.Write(baCreatedFormDocument, 0, baCreatedFormDocument.Length);
            pdfStream.Position = 0;

            Response.AppendHeader("content-disposition", "attachment; filename=form_" + Id + ".pdf");
            return new FileStreamResult(pdfStream, "application/pdf");
        }
    }
}