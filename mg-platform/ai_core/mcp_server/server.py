"""
Serveur MCP Principal pour le projet Marne & Gondoire
Fournit les outils essentiels pour l'analyse et l'enrichissement des données
"""

import asyncio
import json
import logging
from typing import Any, Dict, List, Optional
from pathlib import Path

from fastapi import FastAPI, HTTPException
from fastapi.middleware.cors import CORSMiddleware
from pydantic import BaseModel

# Importation des outils
from tools.file_tools import analyze_file, enrich_file
from tools.data_tools import run_sql, get_table_schema
from tools.scraping_tools import search_web, scrape_url

# Configuration du logging
logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

# Modèles Pydantic pour les requêtes/réponses
class ToolRequest(BaseModel):
    name: str
    arguments: Dict[str, Any]

class ToolResponse(BaseModel):
    success: bool
    result: Any
    error: Optional[str] = None

class MCPServer:
    def __init__(self, name: str, version: str = "0.1.0"):
        self.name = name
        self.version = version
        self.tools = {}
        self.app = FastAPI(
            title=f"{name} MCP Server",
            description="Model Context Protocol Server pour Marne & Gondoire",
            version=version
        )
        
        # Configuration CORS
        self.app.add_middleware(
            CORSMiddleware,
            allow_origins=["*"],
            allow_credentials=True,
            allow_methods=["*"],
            allow_headers=["*"],
        )
        
        # Routes principales
        self.setup_routes()
    
    def setup_routes(self):
        """Configure les routes principales du serveur MCP"""
        
        @self.app.get("/")
        async def root():
            return {
                "name": self.name,
                "version": self.version,
                "protocol": "mcp",
                "tools": list(self.tools.keys())
            }
        
        @self.app.get("/tools")
        async def list_tools():
            """Liste tous les outils disponibles"""
            return {
                "tools": [
                    {
                        "name": name,
                        "description": tool.get("description", ""),
                        "parameters": tool.get("parameters", {})
                    }
                    for name, tool in self.tools.items()
                ]
            }
        
        @self.app.post("/tools/{tool_name}")
        async def call_tool(tool_name: str, request: ToolRequest):
            """Appelle un outil spécifique"""
            if tool_name not in self.tools:
                raise HTTPException(status_code=404, detail=f"Tool '{tool_name}' not found")
            
            try:
                tool_func = self.tools[tool_name]["func"]
                result = await self.execute_tool(tool_func, request.arguments)
                
                return ToolResponse(
                    success=True,
                    result=result
                )
            except Exception as e:
                logger.error(f"Error executing tool {tool_name}: {str(e)}")
                return ToolResponse(
                    success=False,
                    result=None,
                    error=str(e)
                )
    
    async def execute_tool(self, tool_func, arguments: Dict[str, Any]):
        """Exécute un outil avec gestion async/sync"""
        if asyncio.iscoroutinefunction(tool_func):
            return await tool_func(**arguments)
        else:
            return tool_func(**arguments)
    
    def add_tool(self, name: str, func: callable, description: str = "", parameters: Dict = None):
        """Ajoute un outil au serveur MCP"""
        self.tools[name] = {
            "func": func,
            "description": description,
            "parameters": parameters or {}
        }
        logger.info(f"Tool '{name}' registered successfully")


# Création du serveur MCP
server = MCPServer(name="mg-data-mcp")

# Enregistrement des outils
server.add_tool(
    name="analyze_file",
    func=analyze_file,
    description="Analyse un fichier (Excel, CSV, JSON) et identifie les données manquantes",
    parameters={
        "file_path": {"type": "string", "description": "Chemin vers le fichier à analyser"},
        "detailed": {"type": "boolean", "description": "Analyse détaillée (optionnel)", "default": False}
    }
)

server.add_tool(
    name="enrich_file",
    func=enrich_file,
    description="Enrichit un fichier avec des données manquantes via web scraping",
    parameters={
        "file_path": {"type": "string", "description": "Chemin vers le fichier à enrichir"},
        "missing_fields": {"type": "array", "description": "Liste des champs manquants à rechercher"}
    }
)

server.add_tool(
    name="run_sql",
    func=run_sql,
    description="Exécute une requête SQL en lecture seule",
    parameters={
        "query": {"type": "string", "description": "Requête SQL à exécuter"},
        "limit": {"type": "integer", "description": "Limite de résultats (optionnel)", "default": 100}
    }
)

server.add_tool(
    name="get_table_schema",
    func=get_table_schema,
    description="Retourne le schéma d'une table de la base de données",
    parameters={
        "table_name": {"type": "string", "description": "Nom de la table"}
    }
)

server.add_tool(
    name="search_web",
    func=search_web,
    description="Recherche des informations sur le web",
    parameters={
        "query": {"type": "string", "description": "Terme de recherche"},
        "max_results": {"type": "integer", "description": "Nombre maximum de résultats", "default": 5}
    }
)

server.add_tool(
    name="scrape_url",
    func=scrape_url,
    description="Scrape le contenu d'une URL spécifique",
    parameters={
        "url": {"type": "string", "description": "URL à scraper"},
        "extract_fields": {"type": "array", "description": "Champs spécifiques à extraire (optionnel)"}
    }
)

# Application FastAPI
app = server.app

# Route de santé
@app.get("/health")
async def health_check():
    return {"status": "healthy", "server": server.name, "version": server.version}

# Point d'entrée principal
if __name__ == "__main__":
    import uvicorn
    
    logger.info(f"Starting {server.name} v{server.version}")
    logger.info("Available tools:")
    for tool_name in server.tools.keys():
        logger.info(f"  - {tool_name}")
    
    uvicorn.run(
        "server:app",
        host="0.0.0.0",
        port=8080,
        reload=True,
        log_level="info"
    )