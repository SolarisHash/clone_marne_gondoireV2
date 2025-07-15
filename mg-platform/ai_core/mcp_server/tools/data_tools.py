"""
Outils de base de données pour le serveur MCP
Gestion des connexions PostgreSQL et exécution de requêtes
"""

import os
import logging
import pandas as pd
from typing import Dict, List, Any, Optional
from sqlalchemy import create_engine, text, inspect
from sqlalchemy.exc import SQLAlchemyError
from datetime import datetime
import json

logger = logging.getLogger(__name__)

class DatabaseManager:
    """Gestionnaire de base de données avec connexion PostgreSQL"""
    
    def __init__(self):
        self.engine = None
        self.connection_string = self._build_connection_string()
        self._connect()
    
    def _build_connection_string(self) -> str:
        """Construit la chaîne de connexion à partir des variables d'environnement"""
        
        # Valeurs par défaut pour le développement
        defaults = {
            'DB_HOST': 'localhost',
            'DB_PORT': '5432',
            'DB_NAME': 'mg_data',
            'DB_USER': 'mg_user',
            'DB_PASSWORD': 'mg_pass'
        }
        
        # Utilise les variables d'environnement ou les valeurs par défaut
        db_config = {key: os.getenv(key, default) for key, default in defaults.items()}
        
        connection_string = (
            f"postgresql+psycopg2://{db_config['DB_USER']}:{db_config['DB_PASSWORD']}"
            f"@{db_config['DB_HOST']}:{db_config['DB_PORT']}/{db_config['DB_NAME']}"
        )
        
        return connection_string
    
    def _connect(self):
        """Établit la connexion à la base de données"""
        try:
            self.engine = create_engine(
                self.connection_string,
                pool_size=5,
                max_overflow=10,
                pool_pre_ping=True,
                echo=False  # Set to True for SQL debugging
            )
            
            # Test de la connexion
            with self.engine.connect() as conn:
                conn.execute(text("SELECT 1"))
            
            logger.info("Database connection established successfully")
            
        except Exception as e:
            logger.warning(f"Database connection failed: {str(e)}")
            logger.info("Database operations will be simulated")
            self.engine = None
    
    def is_connected(self) -> bool:
        """Vérifie si la connexion à la base de données est active"""
        if not self.engine:
            return False
        
        try:
            with self.engine.connect() as conn:
                conn.execute(text("SELECT 1"))
            return True
        except Exception:
            return False
    
    def execute_query(self, query: str, limit: int = 100) -> Dict[str, Any]:
        """Exécute une requête SQL et retourne les résultats"""
        
        if not self.is_connected():
            return self._simulate_query_result(query, limit)
        
        try:
            # Vérification de sécurité basique
            if not self._is_safe_query(query):
                raise ValueError("Only SELECT queries are allowed")
            
            # Ajout automatique de LIMIT si pas présent
            if 'LIMIT' not in query.upper() and limit > 0:
                query = f"{query.rstrip(';')} LIMIT {limit}"
            
            with self.engine.connect() as conn:
                result = conn.execute(text(query))
                
                # Conversion en format JSON-serializable
                if result.returns_rows:
                    rows = [dict(row._mapping) for row in result]
                    columns = list(result.keys()) if result.keys() else []
                else:
                    rows = []
                    columns = []
                
                return {
                    "success": True,
                    "query": query,
                    "rows": rows,
                    "columns": columns,
                    "row_count": len(rows),
                    "execution_time": datetime.now().isoformat()
                }
                
        except SQLAlchemyError as e:
            logger.error(f"SQL execution error: {str(e)}")
            return {
                "success": False,
                "error": str(e),
                "query": query,
                "rows": [],
                "columns": []
            }
    
    def _is_safe_query(self, query: str) -> bool:
        """Vérifie que la requête est sûre (lecture seule)"""
        query_upper = query.upper().strip()
        
        # Mots-clés interdits
        forbidden_keywords = [
            'INSERT', 'UPDATE', 'DELETE', 'DROP', 'CREATE', 'ALTER', 
            'TRUNCATE', 'GRANT', 'REVOKE', 'EXEC', 'EXECUTE'
        ]
        
        # Vérifie que la requête commence par SELECT
        if not query_upper.startswith('SELECT'):
            return False
        
        # Vérifie l'absence de mots-clés interdits
        for keyword in forbidden_keywords:
            if keyword in query_upper:
                return False
        
        return True
    
    def _simulate_query_result(self, query: str, limit: int) -> Dict[str, Any]:
        """Simule un résultat de requête quand la DB n'est pas disponible"""
        
        # Données simulées selon le type de requête
        if 'files_processed' in query.lower():
            sample_data = [
                {"id": 1, "filename": "sample_data.xlsx", "status": "completed", "created_at": "2024-01-15T10:30:00"},
                {"id": 2, "filename": "customer_list.csv", "status": "processing", "created_at": "2024-01-15T11:15:00"}
            ]
        elif 'enrichment_history' in query.lower():
            sample_data = [
                {"id": 1, "field_name": "email", "original_value": None, "enriched_value": "contact@example.com", "confidence": 0.95},
                {"id": 2, "field_name": "phone", "original_value": None, "enriched_value": "+33123456789", "confidence": 0.88}
            ]
        else:
            sample_data = [
                {"id": 1, "name": "Sample Record", "value": 42, "date": "2024-01-15"},
                {"id": 2, "name": "Another Record", "value": 24, "date": "2024-01-16"}
            ]
        
        # Limite les résultats
        if limit > 0:
            sample_data = sample_data[:limit]
        
        columns = list(sample_data[0].keys()) if sample_data else []
        
        return {
            "success": True,
            "query": query,
            "rows": sample_data,
            "columns": columns,
            "row_count": len(sample_data),
            "execution_time": datetime.now().isoformat(),
            "note": "Simulated data - database not connected"
        }
    
    def get_table_schema(self, table_name: str) -> Dict[str, Any]:
        """Retourne le schéma d'une table"""
        
        if not self.is_connected():
            return self._simulate_table_schema(table_name)
        
        try:
            inspector = inspect(self.engine)
            
            # Vérifier que la table existe
            if table_name not in inspector.get_table_names():
                return {
                    "success": False,
                    "error": f"Table '{table_name}' not found",
                    "table_name": table_name
                }
            
            # Récupérer les colonnes
            columns = inspector.get_columns(table_name)
            
            schema_info = {
                "success": True,
                "table_name": table_name,
                "columns": [
                    {
                        "name": col["name"],
                        "type": str(col["type"]),
                        "nullable": col["nullable"],
                        "default": col.get("default")
                    }
                    for col in columns
                ],
                "indexes": [idx["name"] for idx in inspector.get_indexes(table_name)],
                "foreign_keys": [
                    {
                        "column": fk["constrained_columns"][0],
                        "referenced_table": fk["referred_table"],
                        "referenced_column": fk["referred_columns"][0]
                    }
                    for fk in inspector.get_foreign_keys(table_name)
                ]
            }
            
            return schema_info
            
        except Exception as e:
            logger.error(f"Error getting schema for table {table_name}: {str(e)}")
            return {
                "success": False,
                "error": str(e),
                "table_name": table_name
            }
    
    def _simulate_table_schema(self, table_name: str) -> Dict[str, Any]:
        """Simule le schéma d'une table"""
        
        # Schémas simulés pour les tables principales
        schemas = {
            "files_processed": {
                "columns": [
                    {"name": "id", "type": "INTEGER", "nullable": False, "default": None},
                    {"name": "filename", "type": "VARCHAR(255)", "nullable": False, "default": None},
                    {"name": "status", "type": "VARCHAR(50)", "nullable": False, "default": None},
                    {"name": "analysis_result", "type": "JSONB", "nullable": True, "default": None},
                    {"name": "created_at", "type": "TIMESTAMP", "nullable": False, "default": "NOW()"}
                ],
                "indexes": ["idx_files_status", "idx_files_created_at"],
                "foreign_keys": []
            },
            "enrichment_history": {
                "columns": [
                    {"name": "id", "type": "INTEGER", "nullable": False, "default": None},
                    {"name": "file_id", "type": "INTEGER", "nullable": False, "default": None},
                    {"name": "field_name", "type": "VARCHAR(100)", "nullable": False, "default": None},
                    {"name": "original_value", "type": "TEXT", "nullable": True, "default": None},
                    {"name": "enriched_value", "type": "TEXT", "nullable": True, "default": None},
                    {"name": "source", "type": "VARCHAR(255)", "nullable": True, "default": None},
                    {"name": "confidence", "type": "FLOAT", "nullable": True, "default": None}
                ],
                "indexes": ["idx_enrichment_file_id", "idx_enrichment_field"],
                "foreign_keys": [
                    {"column": "file_id", "referenced_table": "files_processed", "referenced_column": "id"}
                ]
            }
        }
        
        if table_name in schemas:
            return {
                "success": True,
                "table_name": table_name,
                "note": "Simulated schema - database not connected",
                **schemas[table_name]
            }
        else:
            return {
                "success": False,
                "error": f"Table '{table_name}' not found in simulated schemas",
                "table_name": table_name,
                "available_tables": list(schemas.keys())
            }

# Instance globale du gestionnaire de base de données
db_manager = DatabaseManager()

def run_sql(query: str, limit: int = 100) -> Dict[str, Any]:
    """
    Exécute une requête SQL en lecture seule
    
    Args:
        query: Requête SQL à exécuter (SELECT uniquement)
        limit: Nombre maximum de résultats à retourner
    
    Returns:
        Dict contenant les résultats de la requête
    """
    try:
        result = db_manager.execute_query(query, limit)
        logger.info(f"SQL query executed successfully: {query[:50]}...")
        return result
        
    except Exception as e:
        logger.error(f"Error executing SQL query: {str(e)}")
        return {
            "success": False,
            "error": str(e),
            "query": query,
            "rows": [],
            "columns": []
        }

def get_table_schema(table_name: str) -> Dict[str, Any]:
    """
    Retourne le schéma d'une table de la base de données
    
    Args:
        table_name: Nom de la table
    
    Returns:
        Dict contenant le schéma de la table
    """
    try:
        result = db_manager.get_table_schema(table_name)
        logger.info(f"Schema retrieved for table: {table_name}")
        return result
        
    except Exception as e:
        logger.error(f"Error getting schema for table {table_name}: {str(e)}")
        return {
            "success": False,
            "error": str(e),
            "table_name": table_name
        }