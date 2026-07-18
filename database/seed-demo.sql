-- Pakistan-only demo data for Bus Ticket Reservation System
-- WARNING: This resets existing demo bookings, schedules, seats, routes, buses, and passengers.
-- Cities are major Pakistani intercity destinations. Routes are generated in both directions
-- between every listed city so every dropdown combination has demo availability.
-- DistanceKm values are deterministic demo estimates, not verified road distances.

CREATE EXTENSION IF NOT EXISTS "pgcrypto";

TRUNCATE TABLE
    "Tickets",
    "BusSchedules",
    "Seats",
    "Passengers",
    "Routes",
    "Buses"
CASCADE;

INSERT INTO "Buses" ("Id", "CompanyName", "BusName", "TotalSeats", "Price")
VALUES
    (gen_random_uuid(), 'Daewoo Express', 'Daewoo Express PK-01', 45, 3200::numeric),
    (gen_random_uuid(), 'Faisal Movers', 'Faisal Movers PK-01', 45, 3500::numeric),
    (gen_random_uuid(), 'Niazi Express', 'Niazi Express PK-01', 45, 2800::numeric),
    (gen_random_uuid(), 'Road Master', 'Road Master PK-01', 45, 3800::numeric);

INSERT INTO "Seats" ("Id", "BusId", "SeatNumber", "Row", "Status")
SELECT
    gen_random_uuid(),
    bus."Id",
    'S' || seat_number,
    ((seat_number - 1) / 4) + 1,
    0
FROM "Buses" bus
CROSS JOIN generate_series(1, bus."TotalSeats") AS generated_seat(seat_number);

WITH cities("CityNo", "CityName") AS (
    VALUES
        (1, 'Abbottabad'),
        (2, 'Aliabad (Hunza)'),
        (3, 'Attock'),
        (4, 'Badin'),
        (5, 'Bahawalnagar'),
        (6, 'Bahawalpur'),
        (7, 'Bannu'),
        (8, 'Chakwal'),
        (9, 'Chaman'),
        (10, 'Chiniot'),
        (11, 'Dera Ghazi Khan'),
        (12, 'Dera Ismail Khan'),
        (13, 'Faisalabad'),
        (14, 'Gilgit'),
        (15, 'Gujranwala'),
        (16, 'Gujrat'),
        (17, 'Gwadar'),
        (18, 'Hafizabad'),
        (19, 'Hyderabad'),
        (20, 'Islamabad'),
        (21, 'Jacobabad'),
        (22, 'Jhelum'),
        (23, 'Jhang'),
        (24, 'Karachi'),
        (25, 'Kasur'),
        (26, 'Khanewal'),
        (27, 'Khuzdar'),
        (28, 'Kohat'),
        (29, 'Kotli'),
        (30, 'Lahore'),
        (31, 'Larkana'),
        (32, 'Lodhran'),
        (33, 'Loralai'),
        (34, 'Mansehra'),
        (35, 'Mardan'),
        (36, 'Mianwali'),
        (37, 'Mingora'),
        (38, 'Mirpur'),
        (39, 'Mirpur Khas'),
        (40, 'Multan'),
        (41, 'Murree'),
        (42, 'Muzaffarabad'),
        (43, 'Muzaffargarh'),
        (44, 'Narowal'),
        (45, 'Nawabshah'),
        (46, 'Nowshera'),
        (47, 'Okara'),
        (48, 'Peshawar'),
        (49, 'Quetta'),
        (50, 'Rahim Yar Khan'),
        (51, 'Rawalpindi'),
        (52, 'Sahiwal'),
        (53, 'Sargodha'),
        (54, 'Sheikhupura'),
        (55, 'Shikarpur'),
        (56, 'Sialkot'),
        (57, 'Sibi'),
        (58, 'Skardu'),
        (59, 'Sukkur'),
        (60, 'Taxila'),
        (61, 'Thatta'),
        (62, 'Toba Tek Singh'),
        (63, 'Turbat'),
        (64, 'Vehari'),
        (65, 'Wah Cantt'),
        (66, 'Zhob')
), directed_routes AS (
    SELECT
        gen_random_uuid() AS "Id",
        origin."CityName" AS "FromCity",
        destination."CityName" AS "ToCity",
        (80 + ((origin."CityNo" * 97 + destination."CityNo" * 53) % 1250))::double precision AS "DistanceKm",
        origin."CityName" || ' General Bus Stand' AS "BoardingPoint",
        destination."CityName" || ' General Bus Stand' AS "DroppingPoint"
    FROM cities origin
    CROSS JOIN cities destination
    WHERE origin."CityNo" <> destination."CityNo"
)
INSERT INTO "Routes" ("Id", "FromCity", "ToCity", "DistanceKm", "BoardingPoint", "DroppingPoint")
SELECT "Id", "FromCity", "ToCity", "DistanceKm", "BoardingPoint", "DroppingPoint"
FROM directed_routes;

WITH numbered_buses AS (
    SELECT
        "Id",
        row_number() OVER (ORDER BY "CompanyName", "BusName")::integer AS "BusNo",
        count(*) OVER ()::integer AS "BusCount"
    FROM "Buses"
), numbered_routes AS (
    SELECT
        "Id",
        "DistanceKm",
        row_number() OVER (ORDER BY "FromCity", "ToCity")::integer AS "RouteNo"
    FROM "Routes"
), schedule_days AS (
    SELECT generate_series(1, 7)::integer AS "DayOffset"
)
INSERT INTO "BusSchedules" ("Id", "BusId", "RouteId", "JourneyDate", "StartTime", "ArrivalTime")
SELECT
    gen_random_uuid(),
    bus."Id",
    route."Id",
    ((CURRENT_DATE + day."DayOffset") + TIME '00:00') AT TIME ZONE 'Asia/Karachi',
    (
        (CURRENT_DATE + day."DayOffset")
        + TIME '06:00'
        + (((route."RouteNo" + day."DayOffset") % 12) * INTERVAL '1 hour')
    ) AT TIME ZONE 'Asia/Karachi',
    (
        (CURRENT_DATE + day."DayOffset")
        + TIME '06:00'
        + (((route."RouteNo" + day."DayOffset") % 12) * INTERVAL '1 hour')
        + (GREATEST(2, CEIL(route."DistanceKm" / 70.0)::integer) * INTERVAL '1 hour')
    ) AT TIME ZONE 'Asia/Karachi'
FROM numbered_routes route
CROSS JOIN schedule_days day
JOIN numbered_buses bus
  ON bus."BusNo" = 1 + ((route."RouteNo" + day."DayOffset" - 2) % bus."BusCount");