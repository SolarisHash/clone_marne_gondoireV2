"""
Outils d'analyse et d'enrichissement de fichiers
Supporte Excel, CSV, JSON avec détection automatique des données manquantes
"""

import pandas as pd
import json
import logging
from pathlib import Path
from typing import Dict, List, Any, Optional
import numpy as np
from datetime import datetime

logger = logging.getLogger(__name__)

class FileAnalyzer:
    """Classe principale pour l'analyse de fichiers"""
    
    SUPPORTED_FORMATS = ['.xlsx', '.xls', '.csv', '.json']
    
    def __init__(self):
        self.analysis_cache = {}
    
    def detect_file_type(self, file_path: str) -> str:
        """Détecte le type de fichier"""
        path = Path(file_path)
        if not path.exists():
            raise FileNotFoundError(f"File not found: {file_path}")
        
        extension = path.suffix.lower()
        if extension not in self.SUPPORTED_FORMATS:
            raise ValueError(f"Unsupported file format: {extension}")
        
        return extension
    
    def read_file(self, file_path: str) -> pd.DataFrame:
        """Lit un fichier et retourne un DataFrame"""
        file_type = self.detect_file_type(file_path)
        
        try:
            if file_type in ['.xlsx', '.xls']:
                df = pd.read_excel(file_path)
            elif file_type == '.csv':
                # Détection automatique du séparateur
                df = pd.read_csv(file_path, sep=None, engine='python')
            elif file_type == '.json':
                with open(file_path, 'r', encoding='utf-8') as f:
                    data = json.load(f)
                df = pd.json_normalize(data)
            
            logger.info(f"File loaded successfully: {file_path} ({len(df)} rows)")
            return df
            
        except Exception as e:
            logger.error(f"Error reading file {file_path}: {str(e)}")
            raise
    
    def analyze_missing_data(self, df: pd.DataFrame) -> Dict[str, Any]:
        """Analyse les données manquantes"""
        missing_analysis = {}
        
        for column in df.columns:
            missing_count = df[column].isnull().sum()
            missing_percentage = (missing_count / len(df)) * 100
            
            # Détection du type de données
            non_null_series = df[column].dropna()
            if len(non_null_series) > 0:
                dtype = str(non_null_series.dtype)
                sample_values = non_null_series.head(3).tolist()
            else:
                dtype = "unknown"
                sample_values = []
            
            missing_analysis[column] = {
                "missing_count": int(missing_count),
                "missing_percentage": round(missing_percentage, 2),
                "total_rows": len(df),
                "data_type": dtype,
                "sample_values": sample_values,
                "is_critical": missing_percentage > 50  # Plus de 50% manquant = critique
            }
        
        return missing_analysis
    
    def detect_data_patterns(self, df: pd.DataFrame) -> Dict[str, Any]:
        """Détecte des patterns dans les données"""
        patterns = {}
        
        for column in df.columns:
            non_null_data = df[column].dropna()
            
            if len(non_null_data) == 0:
                continue
            
            # Détection du pattern pour chaque colonne
            pattern_info = {
                "unique_values": int(non_null_data.nunique()),
                "most_common": None,
                "pattern_type": "unknown"
            }
            
            # Analyse selon le type
            if non_null_data.dtype == 'object':
                # Données textuelles
                most_common = non_null_data.value_counts().head(1)
                if len(most_common) > 0:
                    pattern_info["most_common"] = {
                        "value": most_common.index[0],
                        "count": int(most_common.iloc[0])
                    }
                
                # Détection de patterns spécifiques
                if column.lower() in ['email', 'mail', 'e-mail']:
                    pattern_info["pattern_type"] = "email"
                elif column.lower() in ['phone', 'telephone', 'tel']:
                    pattern_info["pattern_type"] = "phone"
                elif column.lower() in ['address', 'adresse', 'addr']:
                    pattern_info["pattern_type"] = "address"
                elif column.lower() in ['website', 'site', 'url']:
                    pattern_info["pattern_type"] = "url"
                else:
                    pattern_info["pattern_type"] = "text"
            
            elif pd.api.types.is_numeric_dtype(non_null_data):
                # Données numériques
                pattern_info["pattern_type"] = "numeric"
                pattern_info["min_value"] = float(non_null_data.min())
                pattern_info["max_value"] = float(non_null_data.max())
                pattern_info["mean"] = float(non_null_data.mean())
            
            patterns[column] = pattern_info
        
        return patterns
    
    def generate_enrichment_suggestions(self, missing_analysis: Dict, patterns: Dict) -> List[Dict]:
        """Génère des suggestions d'enrichissement"""
        suggestions = []
        
        for column, missing_info in missing_analysis.items():
            if missing_info["missing_count"] > 0:
                pattern_info = patterns.get(column, {})
                pattern_type = pattern_info.get("pattern_type", "unknown")
                
                suggestion = {
                    "column": column,
                    "missing_count": missing_info["missing_count"],
                    "priority": "high" if missing_info["is_critical"] else "medium",
                    "suggested_actions": []
                }
                
                # Suggestions selon le type de données
                if pattern_type == "email":
                    suggestion["suggested_actions"] = [
                        "Search company website contact pages",
                        "Check professional networks (LinkedIn)",
                        "Use email format patterns from existing data"
                    ]
                elif pattern_type == "phone":
                    suggestion["suggested_actions"] = [
                        "Search company directory",
                        "Check official business listings",
                        "Use phone number validation services"
                    ]
                elif pattern_type == "address":
                    suggestion["suggested_actions"] = [
                        "Use geocoding services",
                        "Search official business registries",
                        "Check Google Maps/Places API"
                    ]
                elif pattern_type == "url":
                    suggestion["suggested_actions"] = [
                        "Search company name + 'website'",
                        "Check domain variations",
                        "Use web directory services"
                    ]
                else:
                    suggestion["suggested_actions"] = [
                        f"Web search for '{column}' information",
                        "Check related databases",
                        "Use data completion services"
                    ]
                
                suggestions.append(suggestion)
        
        return suggestions

# Instance globale de l'analyseur
analyzer = FileAnalyzer()

def analyze_file(file_path: str, detailed: bool = False) -> Dict[str, Any]:
    """
    Analyse un fichier et identifie les données manquantes
    
    Args:
        file_path: Chemin vers le fichier à analyser
        detailed: Si True, retourne une analyse détaillée
    
    Returns:
        Dict contenant l'analyse complète du fichier
    """
    try:
        # Lecture du fichier
        df = analyzer.read_file(file_path)
        
        # Informations de base
        basic_info = {
            "file_path": file_path,
            "file_type": analyzer.detect_file_type(file_path),
            "rows_count": len(df),
            "columns_count": len(df.columns),
            "columns": list(df.columns),
            "analysis_timestamp": datetime.now().isoformat()
        }
        
        # Analyse des données manquantes
        missing_analysis = analyzer.analyze_missing_data(df)
        
        # Résumé des données manquantes
        total_missing = sum(info["missing_count"] for info in missing_analysis.values())
        critical_columns = [col for col, info in missing_analysis.items() if info["is_critical"]]
        
        result = {
            "basic_info": basic_info,
            "missing_data_summary": {
                "total_missing_values": total_missing,
                "columns_with_missing": len([col for col, info in missing_analysis.items() if info["missing_count"] > 0]),
                "critical_columns": critical_columns,
                "completion_rate": round(((len(df) * len(df.columns) - total_missing) / (len(df) * len(df.columns))) * 100, 2)
            },
            "missing_data_details": missing_analysis
        }
        
        # Analyse détaillée si demandée
        if detailed:
            patterns = analyzer.detect_data_patterns(df)
            suggestions = analyzer.generate_enrichment_suggestions(missing_analysis, patterns)
            
            result["data_patterns"] = patterns
            result["enrichment_suggestions"] = suggestions
        
        logger.info(f"File analysis completed for {file_path}")
        return result
        
    except Exception as e:
        logger.error(f"Error analyzing file {file_path}: {str(e)}")
        return {
            "error": str(e),
            "file_path": file_path,
            "success": False
        }

def enrich_file(file_path: str, missing_fields: List[str]) -> Dict[str, Any]:
    """
    Enrichit un fichier avec des données manquantes
    
    Args:
        file_path: Chemin vers le fichier à enrichir
        missing_fields: Liste des champs à enrichir
    
    Returns:
        Dict contenant le résultat de l'enrichissement
    """
    try:
        # Pour l'instant, on retourne une structure de base
        # L'enrichissement réel sera implémenté avec les scrapers
        
        df = analyzer.read_file(file_path)
        
        result = {
            "file_path": file_path,
            "fields_to_enrich": missing_fields,
            "original_rows": len(df),
            "enrichment_status": "pending",
            "message": "Enrichment functionality will be implemented with web scrapers",
            "timestamp": datetime.now().isoformat()
        }
        
        logger.info(f"Enrichment request processed for {file_path}")
        return result
        
    except Exception as e:
        logger.error(f"Error enriching file {file_path}: {str(e)}")
        return {
            "error": str(e),
            "file_path": file_path,
            "success": False
        }