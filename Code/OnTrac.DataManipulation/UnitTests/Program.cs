using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OnTrac.Messaging;
using System.Diagnostics;
using System.Xml;
using OnTrac.DataFile;
using OnTrac.DataManipulation.DelimitedDataFile.FileObject;

namespace OnTrac.Messaging.UnitTests
{
    class Program
    {
        static void Main(string[] args)
        {
            //string input = this._args.GetContent(true, this._args.ArchivePath);
            string data = System.IO.File.ReadAllText(@"C:\Users\sbanjanovic\Desktop\Drop Sites Import.txt");

            DelimitedDataFileSettings ddfs = new DelimitedDataFileSettings
            {
                Delimiter = ',',
                IgnoreFirstLine = true,
                RemoveQuotes = true
            };

            DelimitedDataFile pdd = new DelimitedDataFile(data, ddfs);
            var x = pdd.Records.Where(r => r.RecordIndex == 999);

            DelimitedDataFileBatch pd = new DelimitedDataFileBatch(data, ddfs);

            var uspsResponse = pd.Batches;
            IList<DataSegmentObject<ErrorWarningGroupEnum>> inn = uspsResponse
                        .Select
                            (b =>
                                {
                                    var p = new DataSegmentObject<ErrorWarningGroupEnum>();

                                    for (int k = 0; k < b.Header.Items.Count; k++)
                                    {
                                        p[k] = b.Header.Items[k];
                                    }

                                    //var c = new List<DataSegmentObject<ErrorWarningDetailEnum>>
                                    b.Records.Select(r =>
                                        {
                                            var i = new DataSegmentObject<ErrorWarningDetailEnum>(p);

                                            for (int n = 0; n < r.Items.Count; n++)
                                            {
                                                i[n] = r.Items[n];
                                            }

                                            p.AddOffspring(i);

                                            return i;
                                        }).ToList();

                                    

                                    return p;
                                }
                            ).ToList();

            var u = uspsResponse.Select(s => s.ToString()).ToList();


            var singleItem = uspsResponse.FirstOrDefault();

            var px = new DataSegmentObject<ErrorWarningGroupEnum>();

            for (int k = 0; k < singleItem.Header.Items.Count; k++)
            {
                px.SetValue("ThisIsNotAField", singleItem.Header.Items[k]);
            }
        }       


        private static void TestMethod()
        {
            string fullPathAndFileName = @"C:\Drops\USPS\Metadata\DDU\OnTracDDUSortCodes_072122013.csv";

            // var p = Directory.GetParent(Directory.GetParent(@"C:\Drops\USPS\Metadata\DDU\OnTracDDUSortCodes_072122013.csv").FullName).FullName;

            DirectoryInfo dirDetails = Directory.GetParent(Directory.GetParent(fullPathAndFileName).FullName);

            DirectoryInfo[] directories = dirDetails.GetDirectories();
            string parent = dirDetails.FullName;
            string archive = parent + "\\Archive";
            string name = "\\archive_" + DateTime.Now.ToString("dd-MM-yyyy-hh_mm_ss") + "_" + Path.GetFileName(fullPathAndFileName);

            var l = directories.Where(p => p.Name.ToUpper() == "ARCHIVE").ToList();
            var t = directories.Any(p => p.Name.ToUpper() == "ARCHIVE");
            if (!directories.Any(p => p.Name.ToUpper() == "ARCHIVE"))
            {
                Directory.CreateDirectory(archive);
            }

            File.Copy(fullPathAndFileName, archive + name);



















            DelimitedDataFileSettings basic_settings = new DelimitedDataFileSettings()
            {
                Delimiter = ',',
                RemoveQuotes = false,
                IgnoreFirstLine = true,
            };

            var dduFile = File.ReadAllText(@"C:\Drops\USPS\Metadata\DDU\OnTracDDUSortCodes_072122013.csv").Trim();
            //var dduFile = File.ReadAllText(@"C:\Users\sbanjanovic\Desktop\ddu_smallTest.csv");
            var basicFile = File.ReadAllText(@"C:\Drops\USPS\Detextro1.V15.rpt");
            var nodeNames = "ZIP,Drop Site Key,Drop Ship Type,Drop Site Name,Drop Site Address,Drop Site City,Drop Site State,Drop Site ZIP,Drop SitePlus4,Scheme Name,Scheme ZIP,Region,DDU Hub,Facility Code,Section,Bin".Split(',').Select(n => n.Trim().Replace(" ", "")).ToArray();

            var document = new DelimitedDataFile(dduFile, basic_settings, false);


            var xdoc = document.ToXml(nodeNames);

            DelimitedDataFileSettings batch_settings = new DelimitedDataFileSettings()
            {
                Delimiter = ',',
                RecordsGroupedByHeader = true,
                DetailIdentifier = new string[] { "W", "E" },
                //IgnoreFirstLine = true
            };

















            var batchedFile = File.ReadAllText(@"C:\Drops\USPS\errwrno1.V15.rpt.05020855");


            var b_document = new DelimitedDataFileBatch(batchedFile, batch_settings);

            string header = @"MailerId,ElectronicFileNum,ElectronicFileReceiptDate,EntryFacilityZip,MialingDate,NumberOfRecordsRead,NumberOfRecordsRejected,NumberOfRecordsAccepted,NumberOfD1RecordsAccepted,NumberOfD2RecordsAccepted, SummaryMessage";
            string[] headerLayout = header.Split(',');

            string detail = @"RecordType-ErrorWarning,ElectronicFileNumber,PIC,ErrorLocation,Message";
            string[] detailLayout = detail.Split(',');


            string scanEvent = @"ExtracVersionNumber,IMpb,ElectronicFileNumber,MailerId,MailerName,DestinationZip,DestinationZip_p4,ScanningFacilityZip,ScanningFacilityName,EventCode,EventName,EventDate,EventTime,MailOwnerId,CustomerReferenceNum,DestinationCountryCode,RecipientName,OriginalLabel,UofMCode,Weight,GuaranteedDeliveryDate,GuaranteedDeliveryTime,LogisticsManagerMailerId,Filler";
            string[] scanEventLayout = scanEvent.Split(',');

            var zx = b_document.ToXml(headerLayout, detailLayout);
            var z = b_document.ToXmlEnumerable(headerLayout, detailLayout);

            var ux = document.ToXml(scanEventLayout);
            var u = document.ToXmlEnumerable(scanEventLayout, "ScanEvent");

            Console.ReadLine(); // keep process running

        }
    }


}
