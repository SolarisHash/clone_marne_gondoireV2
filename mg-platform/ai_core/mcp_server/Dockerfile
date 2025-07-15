# ===========================
# DOCKERFILE - SERVEUR MCP
# ===========================

FROM python:3.11-slim

# Métadonnées
LABEL maintainer="Marne & Gondoire Team"
LABEL version="0.1.0"
LABEL description="Serveur MCP pour l'analyse et l'enrichissement de données"

# Variables d'environnement
ENV PYTHONUNBUFFERED=1
ENV PYTHONDONTWRITEBYTECODE=1
ENV PYTHONPATH=/app

# Création de l'utilisateur non-root
RUN groupadd -r appuser && useradd -r -g appuser appuser

# Installation des dépendances système
RUN apt-get update && apt-get install -y \
    curl \
    build-essential \
    libpq-dev \
    && rm -rf /var/lib/apt/lists/*

# Répertoire de travail
WORKDIR /app

# Copie et installation des dépendances Python
COPY requirements.txt .
RUN pip install --no-cache-dir --upgrade pip && \
    pip install --no-cache-dir -r requirements.txt

# Copie du code source
COPY ai_core/ ./ai_core/
COPY infrastructure/ ./infrastructure/
COPY config/ ./config/

# Création des dossiers nécessaires
RUN mkdir -p /app/data /app/logs /app/temp

# Changement de propriétaire
RUN chown -R appuser:appuser /app

# Utilisateur non-root
USER appuser

# Port exposé
EXPOSE 8080

# Healthcheck
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

# Point d'entrée
CMD ["uvicorn", "ai_core.mcp_server.server:app", "--host", "0.0.0.0", "--port", "8080"]