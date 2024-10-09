-- Table: public.email

-- DROP TABLE IF EXISTS public.email;

CREATE TABLE IF NOT EXISTS public.email
(
    id bigint NOT NULL DEFAULT nextval('seq_email_id'::regclass),
    version integer NOT NULL DEFAULT 1,
    item_id bigint NOT NULL,
    address character varying(255) COLLATE pg_catalog."default" NOT NULL,
    description text COLLATE pg_catalog."default",
    verified boolean NOT NULL DEFAULT false,
    CONSTRAINT pk_email PRIMARY KEY (id),
    CONSTRAINT uq_email_address UNIQUE (address),
    CONSTRAINT fk_item_id FOREIGN KEY (item_id)
        REFERENCES public.item (id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE CASCADE
)

TABLESPACE pg_default;

ALTER TABLE IF EXISTS public.email
    OWNER to postgres;
