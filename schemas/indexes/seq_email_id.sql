-- SEQUENCE: public.seq_email_id

-- DROP SEQUENCE IF EXISTS public.seq_email_id;

CREATE SEQUENCE IF NOT EXISTS public.seq_email_id
    INCREMENT 1
    START 1
    MINVALUE 1
    MAXVALUE 9223372036854775807
    CACHE 1;

ALTER SEQUENCE public.seq_email_id
    OWNER TO postgres;
