using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Xsl;

namespace OnTrac.Messaging.Transformers
{
    public class Xslt
    {
        
        /// <summary>
        /// Transforms input Xml using supplied stylesheet
        /// </summary>
        /// <param name="xml">Input Xml to transform</param>
        /// <param name="stylesheet">Compiled stylesheet to be used for transform</param>
        /// <param name="xmlWriterSettings">Optional writer settings</param>
        /// <param name="xmlReaderSettings">Optional reader settings</param>
        /// <returns></returns>
        public static string ApplyXslt(string xml, XslCompiledTransform stylesheet, XmlWriterSettings xmlWriterSettings = null, XmlReaderSettings xmlReaderSettings = null)
        {
            if (xmlWriterSettings == null)
            {
                xmlWriterSettings = new XmlWriterSettings
                {
                    OmitXmlDeclaration = true,
                    Encoding = Encoding.UTF8,
                    Indent = false
                };
            }

            if (xmlReaderSettings == null)
            {
                xmlReaderSettings = new XmlReaderSettings
                {
                    ValidationType = ValidationType.None,
                    CloseInput = true
                };
            }

            StringBuilder sb = new StringBuilder();
            using (StringReader tr = new StringReader(xml))
            {
                using (XmlReader xr = XmlReader.Create(tr, xmlReaderSettings))
                {
                    using (XmlWriter xw = XmlWriter.Create(sb, xmlWriterSettings))
                    {
                        stylesheet.Transform(xr, xw);
                    }
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Transforms input Xml using supplied stylesheet
        /// </summary>
        /// <param name="xml">Input Xml</param>
        /// <param name="styleSheetPath">Stylesheet file path</param>
        /// <param name="checkCharacters">Should character validation occure</param>
        /// <param name="indentOutputXml">Indent output Xml</param>
        /// <returns></returns>
        public static string ApplyXslt(string xml, string styleSheetPath, bool checkCharacters = false, bool indentOutputXml = false)
        {
            XmlWriterSettings xws = new XmlWriterSettings
            {
                OmitXmlDeclaration = true,
                Encoding = Encoding.UTF8,
                Indent = indentOutputXml,
                CheckCharacters = checkCharacters
            };

            XmlReaderSettings xrs = new XmlReaderSettings
            {
                ValidationType = ValidationType.None,
                CheckCharacters = checkCharacters,
                CloseInput = true
            };

            XslCompiledTransform styleSheet = new XslCompiledTransform();
            styleSheet.Load(styleSheetPath);

            return ApplyXslt(xml, styleSheet, xws, xrs);
        }

        /// <summary>
        /// Transforms a set of Xmls into desired format using designated stylehseet
        /// </summary>
        /// <param name="xmlSet">Set of input Xmls</param>
        /// <param name="stylesheet">Stylehseet to be applied to set</param>
        /// <param name="xmlWriterSettings">Optional writer settings</param>
        /// <param name="xmlReaderSettings">Optional reader settings</param>
        /// <returns></returns>
        public static IEnumerable<string> ApplyXslt(IEnumerable<string> xmlSet, XslCompiledTransform stylesheet, XmlWriterSettings xmlWriterSettings = null, XmlReaderSettings xmlReaderSettings = null)
        {                        
            if (xmlWriterSettings == null)
            {
                xmlWriterSettings = new XmlWriterSettings
                {
                    OmitXmlDeclaration = true,
                    Encoding = Encoding.UTF8,
                    Indent = true
                };
            }

            if (xmlReaderSettings == null)
            {
                xmlReaderSettings = new XmlReaderSettings
                {
                    ValidationType = ValidationType.None,
                    CloseInput = true
                };
            }

            ConcurrentBag<string> t_items = new ConcurrentBag<string>();
            Parallel.ForEach(xmlSet, current =>
                {
                    StringBuilder sb = new StringBuilder();
                    using (StringReader tr = new StringReader(current))
                    {
                        using (XmlReader xr = XmlReader.Create(tr, xmlReaderSettings))
                        {
                            using (XmlWriter xw = XmlWriter.Create(sb, xmlWriterSettings))
                            {
                                stylesheet.Transform(xr, xw);
                            }
                        }
                    }

                    t_items.Add(sb.ToString());
                });

            return t_items;
        }
    }
}
