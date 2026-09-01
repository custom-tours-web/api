CREATE TABLE IF NOT EXISTS BookingRequests (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    FullName TEXT NOT NULL CHECK(length(FullName) > 0 AND length(FullName) <= 100),
    PhoneNumber TEXT NOT NULL CHECK(length(PhoneNumber) > 0 AND length(PhoneNumber) <= 20),
    CurrentLocation TEXT NOT NULL CHECK(length(CurrentLocation) > 0 AND length(CurrentLocation) <= 150),
    Destination TEXT NOT NULL CHECK(length(Destination) > 0 AND length(Destination) <= 150),
    TourDate TEXT NOT NULL CHECK(date(TourDate) IS NOT NULL),
    NumberOfMembers INTEGER NOT NULL CHECK(NumberOfMembers >= 1 AND NumberOfMembers <= 100),
    SpecialRequests TEXT CHECK(SpecialRequests IS NULL OR length(SpecialRequests) <= 500),
    Status INTEGER NOT NULL DEFAULT 0 CHECK(Status IN (0, 1, 2, 3)),
    CreatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP CHECK(datetime(CreatedAt) IS NOT NULL),
    UpdatedAt TEXT CHECK(UpdatedAt IS NULL OR datetime(UpdatedAt) IS NOT NULL),
    CHECK (CurrentLocation COLLATE NOCASE != Destination COLLATE NOCASE)
);
