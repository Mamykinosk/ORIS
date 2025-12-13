CREATE SCHEMA IF NOT EXISTS public;

DROP TABLE IF EXISTS public."amenities";
DROP TABLE IF EXISTS public."weatherstats";
DROP TABLE IF EXISTS public."roomoffers";
DROP TABLE IF EXISTS public."hotelimages";
DROP TABLE IF EXISTS public."reviews";
DROP TABLE IF EXISTS public."freedates";
DROP TABLE IF EXISTS public."hoteldetails";
DROP TABLE IF EXISTS public."tours";
DROP TABLE IF EXISTS public."users";

CREATE TABLE public."tours" (
                                "Id" SERIAL PRIMARY KEY,
                                "Name" TEXT,
                                "Location" TEXT,
                                "Image" TEXT,
                                "Price" TEXT,
                                "Date" TEXT,
                                "Duration" TEXT,
                                "MealPlan" TEXT,
                                "IsEarlyBooking" BOOLEAN,
                                "IsDiscount" BOOLEAN,
                                "RatingStars" INT DEFAULT 5,
                                "Type" TEXT, 
                                "Error" text
);

CREATE TABLE public."hoteldetails" (
                                       "Id" SERIAL PRIMARY KEY,
                                       "TourId" INT,
                                       "FoundationYear" INT,
                                       "RenovationYear" TEXT,
                                       "Area" TEXT,
                                       "City" TEXT,
                                       "DistanceCity" TEXT,
                                       "DistanceAirport" TEXT,
                                       "Address" TEXT,
                                       "Phone" TEXT,
                                       "Email" TEXT,
                                       "Site" TEXT,
                                       "Description" TEXT
);

CREATE TABLE public."hotelimages" (
                                      "Id" SERIAL PRIMARY KEY,
                                      "TourId" INT,
                                      "ImageUrl" TEXT,
                                      "Number" INT
);

CREATE TABLE public."roomoffers" (
                                     "Id" SERIAL PRIMARY KEY,
                                     "TourId" INT,
                                     "Image" TEXT,
                                     "Name" TEXT,
                                     "Description" TEXT,
                                     "Dates" TEXT,
                                     "OldPrice" TEXT,
                                     "Price" TEXT,
                                     "DiscountTag" TEXT
);

CREATE TABLE public."weatherstats" (
                                       "Id" SERIAL PRIMARY KEY,
                                       "TourId" INT,
                                       "Month" TEXT,
                                       "AirTemp" INT,
                                       "WaterTemp" INT
);

CREATE TABLE public."amenities" (
                                    "Id" SERIAL PRIMARY KEY,
                                    "TourId" INT,
                                    "Category" TEXT, 
                                    "Title" TEXT,
                                    "Content" TEXT
);

CREATE TABLE public."freedates" (
                                    "Id" SERIAL PRIMARY KEY,
                                    "TourId" INT,
                                    "StartDate" TEXT,
                                    "Duration" TEXT,
                                    "MealPlan" TEXT,
                                    "Price" TEXT
);

CREATE TABLE public."reviews" (
                                  "Id" SERIAL PRIMARY KEY,
                                  "TourId" INT,
                                  "Author" TEXT,
                                  "Date" TEXT,
                                  "TripType" TEXT,
                                  "Title" TEXT,
                                  "Content" TEXT,
                                  "Rating" INT
);

CREATE TABLE public."users" (
                                "Id" SERIAL PRIMARY KEY,
                                "Email" TEXT,
                                "Password" TEXT,
                                "Role" TEXT,     
                                "Name" TEXT
);

INSERT INTO public."users" ("Email", "Password", "Role", "Name")
VALUES ('admin@test.com', '123', 'admin', 'Администратор');

INSERT INTO public."hoteldetails" ("TourId", "FoundationYear", "RenovationYear", "Area", "City", "DistanceCity", "DistanceAirport", "Address", "Phone", "Email", "Site", "Description") VALUES
    (1, 2000, '2018', '184 000 кв.м.', 'Acisu', 'В 6 км от центра Belek', 'В 40 км от аэропорта Antalya-AYT', 'Acısu Mevkii Belek ANTALYA', '+(90) 2427100000', 'info@xanaduresort.com.tr', 'www.xanaduhotels.com.tr', 'Роскошный отель');

INSERT INTO public."hotelimages" ("TourId", "ImageUrl", "Number") VALUES
                                                            (1, 'hotels/desc1.jpg', 2),
                                                            (1, 'hotels/desc2.webp', 3),
                                                            (1, 'hotels/desc3.jpg', 4),
                                                            (1, 'hotels/desc4.jpg', 5);

INSERT INTO public."roomoffers" ("TourId", "Image", "Name", "Description", "Dates", "OldPrice", "Price", "DiscountTag") VALUES
                                                                                                                            (1, 'hotels/room1.webp', 'Main Building Standard Room Garden View', 'Питание и размещение: Хай Класс Все Включено - Взрослых: 2', '17 июня - 30 июня', '157 604,44 Р', '127 537,57 Р', '19% скидка'),
                                                                                                                            (1, 'hotels/room2.jpg', 'Main Building Standard Room Sea View', 'Питание и размещение: Хай Класс Все Включено - Взрослых: 2', '13 августа - 24 августа', '166 505,32 Р', '133 768,60 Р', '19% скидка'),
                                                                                                                            (1, 'hotels/room3.jpg', 'Main Building Junior Suite Garden View', 'Питание и размещение: Хай Класс Все Включено - Взрослых: 2', '19 сентября - 26 сентября', '182 538,46 Р', '145 061,24 Р', '20% скидка');

INSERT INTO public."amenities" ("TourId", "Category", "Title", "Content") VALUES
                                                                              (1, 'InHotel', 'Главное Здание', '1 (этажей: 5, лифтов: 4)'),
                                                                              (1, 'InHotel', 'Рестораны', '7 (из них ресторанов а’ля карт: 2)'),
                                                                              (1, 'Pools', 'Бассейны для взрослых', '2 (Крытый с подогревом, Открытый с подогревом)'),
                                                                              (1, 'Beach', 'Пляж', 'Первая береговая линия, Частный пляж 415 м, Песок');

INSERT INTO public."weatherstats" ("TourId", "Month", "AirTemp", "WaterTemp") VALUES
                                                                                  (1, 'Янв', 13, 20), (1, 'Фев', 13, 18), (1, 'Мар', 15, 18), (1, 'Апр', 20, 20),
                                                                                  (1, 'Май', 21, 22), (1, 'Июн', 29, 27), (1, 'Июл', 31, 29), (1, 'Авг', 29, 30),
                                                                                  (1, 'Сен', 26, 29), (1, 'Окт', 22, 26), (1, 'Ноя', 16, 23), (1, 'Дек', 13, 20);

INSERT INTO public."freedates" ("TourId", "StartDate", "Duration", "MealPlan", "Price") VALUES
                                                                                            (1, '13.12.2025', '6 ночей', 'Все включено', '127 537 Р'),
                                                                                            (1, '14.12.2025', '7 ночей', 'Все включено', '133 768 Р');

INSERT INTO public."reviews" ("TourId", "Author", "Date", "TripType", "Title", "Content", "Rating") VALUES
                                                                                                        (1, 'Yana D', '31 окт 2025 г.', 'Короткий отпуск с друзьями', 'Великолепный отдых', 'Отдых в отеле Xanadu превзошёл все ожидания!', 5),
                                                                                                        (1, 'Марина К', '15 сен 2025 г.', 'Короткий отпуск', 'Незабываемые впечатления', 'Прекрасный отель с потрясающим видом!', 5);


INSERT INTO public."tours" ("Name", "Location", "Image", "Price", "Date", "Duration", "MealPlan", "IsEarlyBooking", "IsDiscount", "Type") VALUES
        ('Xanadu Resort Hotel', 'Турция, Белек', 'hotels/hotel1.jpg', '127 537,57', '13.12.2025', '7 ночей', 'Хай Класс Все Включено', true, true, 'beach'),
        ('Grand Barhan Hotel', 'Турция, Аланья', 'hotels/hotel2.png', '48 714', '13.12.2025 - 19.12.2025', '3 ночи', 'Без питания', true, false, 'hot'),
        ('Isinda', 'Турция, Коньяалты', 'hotels/hotel3.jpg', '51 799', '13.12.2025 - 19.12.2025', '3 ночи', 'Завтрак', true, false, 'hot'),
        ('Benna', 'Турция, Коньяалты', 'hotels/hotel4.jpg', '51 799', '17.01.2025', '3 ночи', 'Завтрак + Ужин', false, false, 'beach'),
        ('Cleopatra Golden Beach', 'Турция, Аланья', 'hotels/hotel5.png', '52 619', '13.01.2025', '3 ночи', 'Все включено', false, true, 'beach'),
        ('Alp Pasa Hotel', 'Турция, Анталья', 'hotels/hotel6.png', '52 786', '17.01.2025', '3 ночи', 'Завтрак', true, false, 'weekend'),
        ('Prenses Sealine', 'Турция, Белек', 'hotels/hotel7.png', '54 008', '17.06.2025', '3 ночи', 'Без питания', true, false, 'hot'),
        ('Sette Serenity Hotel', 'Турция, Аланья', 'hotels/hotel8.png', '54 267', '17.01.2025', '3 ночи', 'Все включено', false, false, 'beach'),
        ('Suite Laguna Hotel', 'Турция, Анталья', 'hotels/hotel9.png', '54 482', '17.04.2025', '3 ночи', 'Без питания', true, false, 'beach'),
        ('Ozbek Hotel', 'Турция, Стамбул', 'hotels/hotel10.png', '54 569', '19.12.2025', '10 ночей', 'Завтрак', false, false, 'weekend'),
        ('Nova City Hotel', 'Турция, Стамбул', 'hotels/hotel11.png', '54 897', '19.12.2025', '10 ночей', 'Завтрак', false, true, 'weekend'),
        ('Deniz Houses Hotel', 'Турция, Стамбул', 'hotels/hotel12.png', '55 035', '19.12.2025', '10 ночей', 'Завтрак', false, true, 'hot');

INSERT INTO public."hoteldetails" ("TourId", "City", "Description", "Address", "Phone", "Site") VALUES
    (2, 'Аланья', 'Уютный отель рядом с пляжем Клеопатра. Идеально для молодежи.', 'Saray Mah, Alanya', '+90 242 513 0002', 'grandbarhan.com');
INSERT INTO public."hotelimages" ("TourId", "ImageUrl") VALUES (2, 'hotels/hotel2.png'), (2, 'hotels/room1.jpg');

INSERT INTO public."hoteldetails" ("TourId", "City", "Description", "Address", "Phone", "Site") VALUES
    (3, 'Анталья', 'Тихий отель в районе Коньяалты. Галечный пляж в 100 метрах.', 'Konyaalti, Antalya', '+90 242 000 0003', 'isinda.com');
INSERT INTO public."hotelimages" ("TourId", "ImageUrl") VALUES (3, 'hotels/hotel3.jpg'), (3, 'hotels/room2.jpg');

INSERT INTO public."hoteldetails" ("TourId", "City", "Description", "Address", "Phone", "Site") VALUES
    (4, 'Анталья', 'Экономичный вариант с бассейном. Вкусные завтраки.', 'Konyaalti, Antalya', '+90 242 000 0004', 'bennahotel.com');
INSERT INTO public."hotelimages" ("TourId", "ImageUrl") VALUES (4, 'hotels/hotel4.jpg'), (4, 'hotels/room3.jpg');

INSERT INTO public."hoteldetails" ("TourId", "City", "Description", "Address", "Phone", "Site") VALUES
    (5, 'Аланья', 'Популярный отель прямо на пляже Клеопатра. Отличный вид на крепость.', 'Ataturk Blv, Alanya', '+90 242 513 0005', 'cleopatrabeach.com');
INSERT INTO public."hotelimages" ("TourId", "ImageUrl") VALUES (5, 'hotels/hotel5.png'), (5, 'hotels/xanadu_1.jpg');

INSERT INTO public."hoteldetails" ("TourId", "City", "Description", "Address", "Phone", "Site") VALUES
    (6, 'Анталья', 'Исторический бутик-отель в старом городе Калеичи. Османский стиль.', 'Kaleici, Antalya', '+90 242 247 0006', 'alppasa.com');
INSERT INTO public."hotelimages" ("TourId", "ImageUrl") VALUES (6, 'hotels/hotel6.png'), (6, 'hotels/room1.jpg');

INSERT INTO public."hoteldetails" ("TourId", "City", "Description", "Address", "Phone", "Site") VALUES
    (7, 'Белек', 'Спокойный отель в районе Богазкент. Подходит для семей.', 'Bogazkent, Serik', '+90 242 000 0007', 'sealine.com');
INSERT INTO public."hotelimages" ("TourId", "ImageUrl") VALUES (7, 'hotels/hotel7.png'), (7, 'hotels/pool.jpg');

INSERT INTO public."hoteldetails" ("TourId", "City", "Description", "Address", "Phone", "Site") VALUES
    (8, 'Аланья', 'Современный городской отель. Новый ремонт и стильные номера.', 'Oba, Alanya', '+90 242 513 0008', 'setteserenity.com');
INSERT INTO public."hotelimages" ("TourId", "ImageUrl") VALUES (8, 'hotels/hotel8.png'), (8, 'hotels/room2.jpg');

INSERT INTO public."hoteldetails" ("TourId", "City", "Description", "Address", "Phone", "Site") VALUES
    (9, 'Анталья', 'Апарт-отель в центре города. Просторные номера с кухней.', 'Muratpasa, Antalya', '+90 242 000 0009', 'suitelaguna.com');
INSERT INTO public."hotelimages" ("TourId", "ImageUrl") VALUES (9, 'hotels/hotel9.png'), (9, 'hotels/room3.jpg');

INSERT INTO public."hoteldetails" ("TourId", "City", "Description", "Address", "Phone", "Site") VALUES
    (10, 'Стамбул', 'Уютный отель в историческом центре. Рядом с Гранд Базаром.', 'Fatih, Istanbul', '+90 212 516 0010', 'ozbekhotel.com');
INSERT INTO public."hotelimages" ("TourId", "ImageUrl") VALUES (10, 'hotels/hotel10.png'), (10, 'hotels/xanadu_3.jpg');

INSERT INTO public."hoteldetails" ("TourId", "City", "Description", "Address", "Phone", "Site") VALUES
    (11, 'Стамбул', 'Бизнес-отель с хорошей транспортной доступностью.', 'Sisli, Istanbul', '+90 212 000 0011', 'novacity.com');
INSERT INTO public."hotelimages" ("TourId", "ImageUrl") VALUES (11, 'hotels/hotel11.png'), (11, 'hotels/room1.jpg');


INSERT INTO public."roomoffers" ("TourId", "Image", "Name", "Description", "Dates", "Price", "DiscountTag")
SELECT "Id", "Image", 'Standard Room', 'Стандартное размещение', 'Даты по запросу', "Price", 'Promo'
FROM public."tours" WHERE "Id" BETWEEN 2 AND 11;

INSERT INTO public."amenities" ("TourId", "Category", "Title", "Content")
SELECT "Id", 'General', 'Интернет', 'Бесплатный Wi-Fi'
FROM public."tours" WHERE "Id" BETWEEN 2 AND 11;

INSERT INTO public."freedates" ("TourId", "StartDate", "Duration", "MealPlan", "Price")
SELECT "Id", "Date", "Duration", "MealPlan", "Price"
FROM public."tours" WHERE "Id" BETWEEN 2 AND 11;

INSERT INTO public."weatherstats" ("TourId", "Month", "AirTemp", "WaterTemp")
SELECT t."Id", 'Июль', 30, 25
FROM public."tours" t WHERE t."Id" BETWEEN 2 AND 11;