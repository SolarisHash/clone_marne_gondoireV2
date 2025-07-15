#!/bin/bash

# ===========================
# SCRIPT DE DÉMARRAGE RAPIDE (Windows Compatible)
# Projet Marne & Gondoire - Serveur MCP
# ===========================

set -e

# Couleurs pour les messages
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Fonction pour afficher les messages
log_info() {
    echo -e "${BLUE}[INFO]${NC} $1"
}

log_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1"
}

log_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

log_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Fonction pour détecter Python sur Windows
detect_python() {
    # Essayer différentes commandes Python
    if command -v python &> /dev/null; then
        PYTHON_CMD="python"
        log_success "Python détecté: python"
    elif command -v python3 &> /dev/null; then
        PYTHON_CMD="python3"
        log_success "Python détecté: python3"
    elif command -v py &> /dev/null; then
        PYTHON_CMD="py"
        log_success "Python détecté: py"
    else
        log_error "Python n'est pas trouvé dans le PATH"
        log_error "Veuillez installer Python ou l'ajouter au PATH"
        exit 1
    fi
    
    # Vérifier la version
    PYTHON_VERSION=$($PYTHON_CMD --version 2>&1)
    log_info "Version Python: $PYTHON_VERSION"
}

# Fonction pour vérifier les prérequis
check_prerequisites() {
    log_info "Vérification des prérequis..."
    
    # Détecter Python
    detect_python
    
    # Vérifier pip
    if ! $PYTHON_CMD -m pip --version &> /dev/null; then
        log_error "pip n'est pas installé ou accessible"
        log_error "Essayez: $PYTHON_CMD -m ensurepip --upgrade"
        exit 1
    fi
    
    # Vérifier Docker (optionnel)
    if command -v docker &> /dev/null; then
        log_success "Docker détecté"
        DOCKER_AVAILABLE=true
    else
        log_warning "Docker non détecté - utilisation du mode développement local"
        DOCKER_AVAILABLE=false
    fi
    
    log_success "Prérequis OK"
}

# Fonction pour créer la structure des dossiers
create_structure() {
    log_info "Création de la structure des dossiers..."
    
    # Dossiers principaux
    mkdir -p ai_core/mcp_server/tools
    mkdir -p ai_core/agents
    mkdir -p ai_core/prompts
    
    mkdir -p infrastructure/data/database
    mkdir -p infrastructure/scrapers
    mkdir -p infrastructure/models
    mkdir -p infrastructure/workflows
    
    mkdir -p interfaces/api
    mkdir -p interfaces/dashboard
    mkdir -p interfaces/notifications
    
    mkdir -p tests
    mkdir -p config
    mkdir -p docker
    mkdir -p data
    mkdir -p logs
    
    log_success "Structure créée"
}

# Fonction pour configurer l'environnement virtuel (Windows compatible)
setup_venv() {
    log_info "Configuration de l'environnement virtuel..."
    
    # Créer l'environnement virtuel
    $PYTHON_CMD -m venv .venv
    
    # Activer l'environnement virtuel (Windows Git Bash)
    if [[ "$OSTYPE" == "msys" || "$OSTYPE" == "win32" ]]; then
        # Windows Git Bash
        source .venv/Scripts/activate
        log_info "Environnement virtuel activé (Windows)"
    else
        # Linux/Mac
        source .venv/bin/activate
        log_info "Environnement virtuel activé (Unix)"
    fi
    
    # Vérifier l'activation
    if [[ "$VIRTUAL_ENV" != "" ]]; then
        log_success "Environnement virtuel actif: $VIRTUAL_ENV"
    else
        log_warning "Environnement virtuel peut ne pas être actif"
    fi
    
    # Mettre à jour pip
    $PYTHON_CMD -m pip install --upgrade pip
    
    # Installer les dépendances
    if [[ -f "requirements.txt" ]]; then
        log_info "Installation des dépendances..."
        $PYTHON_CMD -m pip install -r requirements.txt
    else
        log_warning "requirements.txt non trouvé - installation des dépendances de base"
        $PYTHON_CMD -m pip install fastapi uvicorn pandas openpyxl requests beautifulsoup4 sqlalchemy psycopg2-binary
    fi
    
    log_success "Environnement virtuel configuré"
}

# Fonction pour créer les fichiers de configuration
create_config() {
    log_info "Création des fichiers de configuration..."
    
    # Fichier .env
    cat > .env << EOF
# Configuration de base
ENVIRONMENT=development
LOG_LEVEL=INFO

# Base de données
DB_HOST=localhost
DB_PORT=5432
DB_NAME=mg_data
DB_USER=mg_user
DB_PASSWORD=mg_pass

# Redis
REDIS_HOST=localhost
REDIS_PORT=6379

# Serveur MCP
MCP_SERVER_HOST=0.0.0.0
MCP_SERVER_PORT=8080

# Sécurité
SECRET_KEY=your-secret-key-here
API_KEY=your-api-key-here
EOF

    # Créer le dossier config s'il n'existe pas
    mkdir -p config
    
    # Fichier de configuration logging
    cat > config/logging.json << EOF
{
    "version": 1,
    "disable_existing_loggers": false,
    "formatters": {
        "standard": {
            "format": "%(asctime)s [%(levelname)s] %(name)s: %(message)s"
        }
    },
    "handlers": {
        "console": {
            "class": "logging.StreamHandler",
            "level": "INFO",
            "formatter": "standard",
            "stream": "ext://sys.stdout"
        },
        "file": {
            "class": "logging.FileHandler",
            "level": "DEBUG",
            "formatter": "standard",
            "filename": "logs/mcp_server.log"
        }
    },
    "root": {
        "level": "INFO",
        "handlers": ["console", "file"]
    }
}
EOF

    log_success "Configuration créée"
}

# Fonction pour créer le schéma de base de données
create_database_schema() {
    log_info "Création du schéma de base de données..."
    
    mkdir -p infrastructure/data/database
    
    cat > infrastructure/data/database/schema.sql << EOF
-- Schéma de base de données pour Marne & Gondoire
-- Version: 0.1.0

-- Table des fichiers traités
CREATE TABLE IF NOT EXISTS files_processed (
    id SERIAL PRIMARY KEY,
    filename VARCHAR(255) NOT NULL,
    file_path VARCHAR(500) NOT NULL,
    file_size BIGINT,
    file_type VARCHAR(50),
    status VARCHAR(50) DEFAULT 'pending',
    analysis_result JSONB,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    processed_at TIMESTAMP
);

-- Table de l'historique d'enrichissement
CREATE TABLE IF NOT EXISTS enrichment_history (
    id SERIAL PRIMARY KEY,
    file_id INTEGER REFERENCES files_processed(id) ON DELETE CASCADE,
    field_name VARCHAR(100) NOT NULL,
    row_index INTEGER,
    original_value TEXT,
    enriched_value TEXT,
    source VARCHAR(255),
    confidence FLOAT CHECK (confidence >= 0 AND confidence <= 1),
    method VARCHAR(100),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Index pour optimiser les performances
CREATE INDEX IF NOT EXISTS idx_files_status ON files_processed(status);
CREATE INDEX IF NOT EXISTS idx_files_created_at ON files_processed(created_at);
CREATE INDEX IF NOT EXISTS idx_enrichment_file_id ON enrichment_history(file_id);
CREATE INDEX IF NOT EXISTS idx_enrichment_field ON enrichment_history(field_name);
EOF

    log_success "Schéma de base de données créé"
}

# Fonction pour créer un fichier de test simple
create_test_file() {
    log_info "Création du fichier de test..."
    
    cat > test_server.py << EOF
#!/usr/bin/env python3
"""
Test simple du serveur MCP
"""

import requests
import time
import sys

def test_server():
    """Test basique du serveur"""
    url = "http://localhost:8080"
    
    print("🧪 Test du serveur MCP...")
    print(f"URL: {url}")
    
    # Attendre que le serveur soit prêt
    print("⏳ Attente du serveur...")
    for i in range(30):
        try:
            response = requests.get(f"{url}/health", timeout=2)
            if response.status_code == 200:
                print("✅ Serveur en ligne!")
                break
        except requests.exceptions.RequestException:
            pass
        time.sleep(1)
    else:
        print("❌ Serveur non accessible")
        return False
    
    # Test des outils
    try:
        response = requests.get(f"{url}/tools", timeout=5)
        if response.status_code == 200:
            tools = response.json()
            print(f"✅ Outils disponibles: {len(tools.get('tools', []))}")
            return True
        else:
            print(f"❌ Erreur outils: {response.status_code}")
            return False
    except Exception as e:
        print(f"❌ Erreur test: {e}")
        return False

if __name__ == "__main__":
    success = test_server()
    sys.exit(0 if success else 1)
EOF

    chmod +x test_server.py
    log_success "Fichier de test créé"
}

# Fonction pour créer les fichiers Python de base
create_basic_python_files() {
    log_info "Création des fichiers Python de base..."
    
    # Créer __init__.py
    touch ai_core/__init__.py
    touch ai_core/mcp_server/__init__.py
    touch ai_core/mcp_server/tools/__init__.py
    touch ai_core/agents/__init__.py
    touch infrastructure/__init__.py
    
    # Créer un serveur MCP minimal si il n'existe pas
    if [[ ! -f "ai_core/mcp_server/server.py" ]]; then
        log_warning "server.py non trouvé - création d'un serveur minimal"
        
        cat > ai_core/mcp_server/server.py << EOF
"""
Serveur MCP minimal pour test
"""

from fastapi import FastAPI
import uvicorn

app = FastAPI(title="MG Data MCP Server", version="0.1.0")

@app.get("/")
async def root():
    return {"message": "Serveur MCP Marne & Gondoire", "status": "running"}

@app.get("/health")
async def health():
    return {"status": "healthy"}

@app.get("/tools")
async def tools():
    return {"tools": []}

if __name__ == "__main__":
    uvicorn.run(app, host="0.0.0.0", port=8080)
EOF
    fi
    
    log_success "Fichiers Python de base créés"
}

# Fonction pour démarrer le serveur en mode développement
start_dev_server() {
    log_info "Démarrage du serveur en mode développement..."
    
    # Activer l'environnement virtuel
    if [[ "$OSTYPE" == "msys" || "$OSTYPE" == "win32" ]]; then
        source .venv/Scripts/activate
    else
        source .venv/bin/activate
    fi
    
    # Vérifier que le serveur existe
    if [[ ! -f "ai_core/mcp_server/server.py" ]]; then
        log_error "ai_core/mcp_server/server.py non trouvé"
        log_error "Veuillez copier le fichier server.py dans le bon dossier"
        exit 1
    fi
    
    # Démarrer le serveur
    log_info "Serveur MCP démarré sur http://localhost:8080"
    log_info "Appuyez sur Ctrl+C pour arrêter"
    
    cd ai_core/mcp_server
    $PYTHON_CMD server.py &
    SERVER_PID=$!
    
    # Revenir au dossier racine
    cd ../..
    
    # Attendre un peu pour que le serveur démarre
    sleep 5
    
    # Lancer les tests
    log_info "Lancement des tests..."
    $PYTHON_CMD test_server.py
    
    # Demander à l'utilisateur s'il veut arrêter
    echo ""
    log_info "Serveur en cours d'exécution (PID: $SERVER_PID)"
    log_info "Pour arrêter le serveur, appuyez sur Ctrl+C ou fermez ce terminal"
    
    # Attendre l'interruption
    wait $SERVER_PID
}

# Fonction principale
main() {
    echo ""
    echo "🚀 MARNE & GONDOIRE - SERVEUR MCP (Windows Compatible)"
    echo "======================================================"
    echo ""
    
    # Vérifier les arguments
    MODE=${1:-"dev"}
    
    case $MODE in
        "dev"|"development")
            log_info "Mode: Développement local"
            check_prerequisites
            create_structure
            create_config
            create_database_schema
            create_basic_python_files
            create_test_file
            setup_venv
            start_dev_server
            ;;
        "test")
            log_info "Mode: Test uniquement"
            check_prerequisites
            create_test_file
            $PYTHON_CMD test_server.py
            ;;
        "setup")
            log_info "Mode: Setup uniquement"
            check_prerequisites
            create_structure
            create_config
            create_database_schema
            create_basic_python_files
            create_test_file
            setup_venv
            log_success "Setup terminé. Lancez './quick_start.sh dev' pour démarrer"
            ;;
        *)
            echo "Usage: $0 [dev|test|setup]"
            echo ""
            echo "  dev    - Démarrage en mode développement (défaut)"
            echo "  test   - Tests uniquement"
            echo "  setup  - Configuration uniquement"
            echo ""
            exit 1
            ;;
    esac
}

# Gestion des signaux
trap 'log_warning "Arrêt du script..."; kill $SERVER_PID 2>/dev/null || true; exit 0' SIGINT SIGTERM

# Exécution du script principal
main "$@"