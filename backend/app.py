import os
import psycopg2
from fastapi import FastAPI
from pydantic import BaseModel

DB_HOST = os.getenv("DB_HOST", "db")
DB_PORT = int(os.getenv("DB_PORT", "5432"))
DB_NAME = os.getenv("DB_NAME", "appdb")
DB_USER = os.getenv("DB_USER", "appuser")
DB_PASSWORD = os.getenv("DB_PASSWORD", "apppass")

def get_conn():
    return psycopg2.connect(
        host=DB_HOST, port=DB_PORT, dbname=DB_NAME, user=DB_USER, password=DB_PASSWORD
    )

app = FastAPI(title="python-3tier-postgres")

def ensure_db():
    with get_conn() as conn:
        with conn.cursor() as cur:
            cur.execute("CREATE TABLE IF NOT EXISTS items (id SERIAL PRIMARY KEY, name TEXT NOT NULL);")
            cur.execute("SELECT COUNT(*) FROM items;")
            count = cur.fetchone()[0]
            if count == 0:
                cur.execute("INSERT INTO items(name) VALUES (%s), (%s);",
                            ("Hello from Python backend", "Postgres is connected"))
        conn.commit()

ensure_db()

class ItemIn(BaseModel):
    name: str | None = None

@app.get("/api/healthz")
def healthz():
    with get_conn() as conn:
        with conn.cursor() as cur:
            cur.execute("SELECT 1;")
            one = cur.fetchone()[0]
    return {"status": "ok", "db": one}

@app.get("/api/items")
def items():
    with get_conn() as conn:
        with conn.cursor() as cur:
            cur.execute("SELECT id, name FROM items ORDER BY id;")
            rows = cur.fetchall()
    return [{"id": r[0], "name": r[1]} for r in rows]

@app.post("/api/items")
def add(item: ItemIn):
    name = item.name or "Unnamed"
    with get_conn() as conn:
        with conn.cursor() as cur:
            cur.execute("INSERT INTO items(name) VALUES (%s);", (name,))
        conn.commit()
    return {"name": name}
