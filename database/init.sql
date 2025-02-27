-- Create database only if it doesn't exist
DO $$ 
BEGIN
   IF NOT EXISTS (SELECT FROM pg_database WHERE datname = 'quantscopedb') THEN
      CREATE DATABASE quantscopedb;
   END IF;
END $$;

-- Create user if needed
DO $$
BEGIN
   IF NOT EXISTS (SELECT FROM pg_roles WHERE rolname = 'quantuser') THEN
      CREATE USER quantuser WITH PASSWORD 'quantpassword';
   END IF;
END $$;

-- Grant privileges
GRANT ALL PRIVILEGES ON DATABASE quantscopedb TO quantuser;