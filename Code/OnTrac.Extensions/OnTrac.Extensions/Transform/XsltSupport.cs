using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Xsl;

namespace OnTrac.Extensions.Transform    
{
    public static class XsltSupport
    {
        public static string ApplyXslt(string xmlInput, string xsltMapping)
        {
            XslCompiledTransform xsltMapper = null;
            StringBuilder sb = null;

            XmlWriterSettings xws = new XmlWriterSettings
            {
                OmitXmlDeclaration = true,
                Encoding = Encoding.UTF8,
                Indent = false, 
                CheckCharacters = false               
            };

            XmlReaderSettings xrs = new XmlReaderSettings
            {
                CloseInput = true
            };

            XsltSettings xsltSettings = new XsltSettings
            {
                EnableScript = true
            };

            sb = new StringBuilder();
            xsltMapper = new XslCompiledTransform();
            using (TextReader tr = new StringReader(xmlInput))
            {// text reader to store the xml input
                using (XmlReader input = XmlReader.Create(tr))
                {// xml reader used to process xml input
                    using (TextReader styleSheet = new StringReader(xsltMapping))
                    {// text reader for storing xslt mapping
                        using (XmlReader stxr = XmlReader.Create(styleSheet))
                        {// xml reader for processing xslt
                            using (XmlWriter output = XmlWriter.Create(sb))
                            {
                                xsltMapper.Load(stxr, xsltSettings, new XmlUrlResolver());
                                xsltMapper.Transform(input, output);
                            }
                        }
                    }
                }
            }

            return sb.ToString();            
        }
    }
}
