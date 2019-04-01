using System;
using System.Web;
using System.Xml.Linq;
using Microsoft.Web.XmlTransform;
using Umbraco.Core;
using Umbraco.Core.PackageActions;
using Umbraco.Web.Composing;

namespace Slimsy.Packaging
{
    public class PackageActions
    {
        public class TransformConfig : IPackageAction
        {
            public static T AttributeValue<T>(XElement xml, string attributeName)
            {
                if (xml == null) throw new ArgumentNullException("xml");
                if (xml.HasAttributes == false) return default(T);

                if (xml.Attribute(attributeName) == null)
                    return default(T);

                var val = xml.Attribute(attributeName).Value;
                var result = val.TryConvertTo<T>();
                if (result.Success)
                    return result.Result;

                return default(T);
            }

            public string Alias()
            {
                return "Slimsy.TransformConfig";
            }

            public bool Execute(string packageName, XElement xmlData)
            {
                return this.Transform(packageName, xmlData);
            }

            public bool Undo(string packageName, XElement xmlData)
            {
                return this.Transform(packageName, xmlData, true);
            }

            private bool Transform(string packageName, XElement xmlData, bool uninstall = false)
            {
                // The config file we want to modify
                if (xmlData.Attributes() != null)
                {
                    var file = AttributeValue<string>(xmlData, "file");

                    var sourceDocFileName = VirtualPathUtility.ToAbsolute(file);

                    // The xdt file used for tranformation
                    var fileEnd = "install.xdt";
                    if (uninstall)
                    {
                        fileEnd = string.Format("un{0}", fileEnd);
                    }

                    var xdtfile = string.Format("{0}.{1}", AttributeValue<string>(xmlData, "xdtfile"), fileEnd);
                    var xdtFileName = VirtualPathUtility.ToAbsolute(xdtfile);

                    // The translation at-hand
                    using (var xmlDoc = new XmlTransformableDocument())
                    {
                        xmlDoc.PreserveWhitespace = true;
                        xmlDoc.Load(HttpContext.Current.Server.MapPath(sourceDocFileName));

                        using (var xmlTrans = new XmlTransformation(HttpContext.Current.Server.MapPath(xdtFileName)))
                        {
                            if (xmlTrans.Apply(xmlDoc))
                            {
                                // If we made it here, sourceDoc now has transDoc's changes
                                // applied. So, we're going to save the final result off to
                                // destDoc.
                                try
                                {
                                    xmlDoc.Save(HttpContext.Current.Server.MapPath(sourceDocFileName));
                                }
                                catch (Exception e)
                                {
                                    // Log error message
                                    var message = "Error executing TransformConfig package action (check file write permissions): " + e.Message;
                                    Current.Logger.Error(typeof(TransformConfig), message, e);
                                    return false;
                                }
                            }
                        }
                    }
                }

                return true;
            }
        }
    }
}
