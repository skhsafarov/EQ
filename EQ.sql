CREATE TABLE "Users"
(
    "Id" serial PRIMARY KEY,
    "FirstName" character varying(50) NOT NULL,
    "LastName" character varying(50),
    "Role" character varying(50) NOT NULL DEFAULT User
);

CREATE TABLE "Signins"
(
    "Phone" character varying(50) NOT NULL,
    "Code" character varying(6),
    "Attempts" integer,
    "Expiration" timestamp with time zone,
    "UserId" int REFERENCES "Users"("Id")
);

CREATE TABLE "Queues"
(
    "Number" int,
    "UserId" int REFERENCES "Users"("Id") NOT NULL
);
