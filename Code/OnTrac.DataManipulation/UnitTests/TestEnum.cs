using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnTrac.Messaging.UnitTests
{
    internal enum ErrorWarningGroupEnum
    {
        MailerId,
        ElectronicFileSequenceNumber,
        ElectronicFileReceiptDate,
        ElectronicFileReceiptTime,
        EntryFacilityZip,
        MailingDate,
        NumberOfRecordsRead,
        NumberOfRecordsRejected,
        NumberOfRecordsAccepted,
        NumberOfElectronicFileD1RecordsAccepted,
        NumberOfElectronicFileD2RecordsAccepted,
        Message
    }

    internal enum ErrorWarningDetailEnum
    {
        RecordType,
        ElectronicFileLineNumber,
        PackageIdentificationCode,
        FieldContainingError,
        Message
    }
}
