FROM python:3.11-slim
WORKDIR /app
ENV PYTHONDONTWRITEBYTECODE=1
ENV PYTHONUNBUFFERED=1
RUN useradd -m appuser
USER appuser
COPY backend/requirements.txt /app/requirements.txt
RUN pip install --no-cache-dir -r /app/requirements.txt
COPY backend/app.py /app/app.py
EXPOSE 8080
CMD ["python","-m","uvicorn","app:app","--host","0.0.0.0","--port","8080"]
