namespace arna.HRMS.Models.Enums;

public enum AttendanceLocation
{
    Office = 1,
    Remote = 2,
    ClientSite = 3
}

public enum AttendanceReasonType
{
    ForgotToClockIn = 1,
    ForgotToClockOut = 2,
    LateClockIn = 3,
    EarlyClockOut = 4,
    ClientVisit = 5,
    WorkFromHome = 6,
    TechnicalIssue = 7,
    CompOffRequest = 8
}


public enum AttendanceStatus
{
    Present = 1,
    Absent = 2,
    Late = 3,
    HalfDay = 4,
    OnLeave = 5,
    Holiday = 6
}
