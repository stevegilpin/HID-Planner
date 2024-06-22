namespace HIDLib;

[Flags]
public enum Grades
{
    None = 0,
    K0 = 1 << 0,
    K1 = 1 << 1,
    K2 = 1 << 2,
    K3 = 1 << 3,
    K4 = 1 << 4,
    K5 = 1 << 5,
    First = 1 << 6,
    Second = 1 << 7,
    Third = 1 << 8,
    Fourth = 1 << 9,
    Fifth = 1 << 10,
    Sixth = 1 << 11,
    Seventh = 1 << 12,
    Eighth = 1 << 13,
    Ninth = 1 << 14,
    Tenth = 1 << 15,
    Eleventh = 1 << 16,
    Twelfth = 1 << 17,
}