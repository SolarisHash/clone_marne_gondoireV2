"""
Outils de scraping web pour le serveur MCP
Recherche et extraction d'informations depuis le web
"""

import requests
import logging
from typing import Dict, List, Any, Optional
from urllib.parse import urljoin, urlparse
import time
import json
from datetime import datetime
import re
from bs4 import BeautifulSoup

logger = logging.getLogger(__name__)

class WebScraper:
    """Classe principale pour le scraping web"""
    
    def __init__(self):
        self.session = requests.Session()
        self.session.headers.update({
            'User-Agent': 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36'
        })
        self.request_delay = 1  # Délai entre les requêtes (en secondes)
        self.last_request_time = 0
    
    def _respect_rate_limit(self):
        """Respecte les limites de fréquence des requêtes"""
        current_time = time.time()
        time_since_last_request = current_time - self.last_request_time
        
        if time_since_last_request < self.request_delay:
            time.sleep(self.request_delay - time_since_last_request)
        
        self.last_request_time = time.time()
    
    def search_google(self, query: str, max_results: int = 5) -> List[Dict[str, Any]]:
        """
        Effectue une recherche Google (simulation)
        Note: En production, utiliser Google Search API ou alternative
        """
        self._respect_rate_limit()
        
        # Simulation d'une recherche Google
        # En production, remplacer par une vraie API de recherche
        search_results = []
        
        try:
            # Pour l'instant, on simule des résultats
            # TODO: Intégrer une vraie API de recherche (Google, Bing, etc.)
            
            simulated_results = [
                {
                    "title": f"Result for '{query}' - Page 1",
                    "url": f"https://example.com/page1?q={query.replace(' ', '+')}",
                    "description": f"This is a simulated search result for the query '{query}'. It contains relevant information about the search topic.",
                    "source": "example.com"
                },
                {
                    "title": f"Information about '{query}' - Resource 2",
                    "url": f"https://info-site.com/resource?search={query.replace(' ', '-')}",
                    "description": f"Additional information and details about '{query}' can be found here.",
                    "source": "info-site.com"
                },
                {
                    "title": f"Complete guide to '{query}'",
                    "url": f"https://guide.com/topics/{query.replace(' ', '-').lower()}",
                    "description": f"Comprehensive guide and tutorial about '{query}' with examples and best practices.",
                    "source": "guide.com"
                }
            ]
            
            # Limite les résultats
            search_results = simulated_results[:max_results]
            
            logger.info(f"Search completed for query: '{query}' ({len(search_results)} results)")
            
        except Exception as e:
            logger.error(f"Error during search for '{query}': {str(e)}")
            search_results = []
        
        return search_results
    
    def scrape_url(self, url: str, extract_fields: Optional[List[str]] = None) -> Dict[str, Any]:
        """
        Scrape le contenu d'une URL spécifique
        
        Args:
            url: URL à scraper
            extract_fields: Champs spécifiques à extraire (optionnel)
        
        Returns:
            Dict contenant le contenu extrait
        """
        self._respect_rate_limit()
        
        try:
            response = self.session.get(url, timeout=10)
            response.raise_for_status()
            
            # Parse HTML
            soup = BeautifulSoup(response.content, 'html.parser')
            
            # Extraction basique du contenu
            extracted_data = {
                "url": url,
                "title": self._extract_title(soup),
                "description": self._extract_description(soup),
                "text_content": self._extract_text_content(soup),
                "links": self._extract_links(soup, url),
                "images": self._extract_images(soup, url),
                "metadata": self._extract_metadata(soup),
                "extraction_timestamp": datetime.now().isoformat()
            }
            
            # Extraction de champs spécifiques si demandé
            if extract_fields:
                specific_data = {}
                for field in extract_fields:
                    specific_data[field] = self._extract_specific_field(soup, field)
                extracted_data["specific_fields"] = specific_data
            
            logger.info(f"Successfully scraped content from: {url}")
            return extracted_data
            
        except requests.exceptions.RequestException as e:
            logger.error(f"Request error for URL {url}: {str(e)}")
            return {
                "url": url,
                "error": f"Request failed: {str(e)}",
                "success": False
            }
        except Exception as e:
            logger.error(f"Scraping error for URL {url}: {str(e)}")
            return {
                "url": url,
                "error": f"Scraping failed: {str(e)}",
                "success": False
            }
    
    def _extract_title(self, soup: BeautifulSoup) -> str:
        """Extrait le titre de la page"""
        title_tag = soup.find('title')
        return title_tag.get_text().strip() if title_tag else "No title found"
    
    def _extract_description(self, soup: BeautifulSoup) -> str:
        """Extrait la description de la page"""
        # Cherche la meta description
        meta_desc = soup.find('meta', attrs={'name': 'description'})
        if meta_desc and meta_desc.get('content'):
            return meta_desc['content'].strip()
        
        # Fallback: premier paragraphe
        first_p = soup.find('p')
        if first_p:
            return first_p.get_text().strip()[:200] + "..."
        
        return "No description found"
    
    def _extract_text_content(self, soup: BeautifulSoup) -> str:
        """Extrait le contenu textuel principal"""
        # Supprime les scripts et styles
        for script in soup(["script", "style"]):
            script.decompose()
        
        # Extrait le texte
        text = soup.get_text()
        
        # Nettoie le texte
        lines = (line.strip() for line in text.splitlines())
        chunks = (phrase.strip() for line in lines for phrase in line.split("  "))
        text = ' '.join(chunk for chunk in chunks if chunk)
        
        # Limite la longueur
        return text[:1000] + "..." if len(text) > 1000 else text
    
    def _extract_links(self, soup: BeautifulSoup, base_url: str) -> List[Dict[str, str]]:
        """Extrait les liens de la page"""
        links = []
        
        for link in soup.find_all('a', href=True):
            href = link['href']
            text = link.get_text().strip()
            
            # Convertit les liens relatifs en absolus
            full_url = urljoin(base_url, href)
            
            if text and len(text) > 0:
                links.append({
                    "url": full_url,
                    "text": text[:100],  # Limite la longueur
                    "is_external": urlparse(full_url).netloc != urlparse(base_url).netloc
                })
        
        return links[:10]  # Limite à 10 liens
    
    def _extract_images(self, soup: BeautifulSoup, base_url: str) -> List[Dict[str, str]]:
        """Extrait les images de la page"""
        images = []
        
        for img in soup.find_all('img', src=True):
            src = img['src']
            alt = img.get('alt', '')
            
            # Convertit les liens relatifs en absolus
            full_url = urljoin(base_url, src)
            
            images.append({
                "url": full_url,
                "alt": alt,
                "width": img.get('width', ''),
                "height": img.get('height', '')
            })
        
        return images[:5]  # Limite à 5 images
    
    def _extract_metadata(self, soup: BeautifulSoup) -> Dict[str, str]:
        """Extrait les métadonnées de la page"""
        metadata = {}
        
        # Meta tags
        for meta in soup.find_all('meta'):
            name = meta.get('name') or meta.get('property')
            content = meta.get('content')
            
            if name and content:
                metadata[name] = content
        
        return metadata
    
    def _extract_specific_field(self, soup: BeautifulSoup, field_name: str) -> str:
        """Extrait un champ spécifique selon son nom"""
        field_lower = field_name.lower()
        
        # Patterns de recherche pour différents types de champs
        patterns = {
            'email': [
                r'\b[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Z|a-z]{2,}\b',
                r'mailto:([^"\'>\s]+)'
            ],
            'phone': [
                r'\+?\d{1,3}[-.\s]?\d{1,3}[-.\s]?\d{1,4}[-.\s]?\d{1,4}[-.\s]?\d{1,9}',
                r'tel:([^"\'>\s]+)'
            ],
            'address': [
                r'\d+\s+[A-Za-z0-9\s,.-]+',
            ]
        }
        
        # Recherche dans le texte
        text = soup.get_text()
        
        if field_lower in patterns:
            for pattern in patterns[field_lower]:
                matches = re.findall(pattern, text)
                if matches:
                    return matches[0] if isinstance(matches[0], str) else matches[0][0]
        
        # Recherche dans les attributs HTML
        selectors = {
            'email': ['[href^="mailto:"]', '[data-email]'],
            'phone': ['[href^="tel:"]', '[data-phone]'],
            'address': ['[data-address]', '.address']
        }
        
        if field_lower in selectors:
            for selector in selectors[field_lower]:
                element = soup.select_one(selector)
                if element:
                    return element.get_text().strip() or element.get('href', '').replace('mailto:', '').replace('tel:', '')
        
        return f"No {field_name} found"

# Instance globale du scraper
scraper = WebScraper()

def search_web(query: str, max_results: int = 5) -> Dict[str, Any]:
    """
    Recherche des informations sur le web
    
    Args:
        query: Terme de recherche
        max_results: Nombre maximum de résultats
    
    Returns:
        Dict contenant les résultats de recherche
    """
    try:
        results = scraper.search_google(query, max_results)
        
        return {
            "success": True,
            "query": query,
            "results": results,
            "result_count": len(results),
            "search_timestamp": datetime.now().isoformat()
        }
        
    except Exception as e:
        logger.error(f"Error during web search for '{query}': {str(e)}")
        return {
            "success": False,
            "query": query,
            "error": str(e),
            "results": []
        }

def scrape_url(url: str, extract_fields: Optional[List[str]] = None) -> Dict[str, Any]:
    """
    Scrape le contenu d'une URL spécifique
    
    Args:
        url: URL à scraper
        extract_fields: Champs spécifiques à extraire (optionnel)
    
    Returns:
        Dict contenant le contenu extrait
    """
    try:
        result = scraper.scrape_url(url, extract_fields)
        result["success"] = "error" not in result
        return result
        
    except Exception as e:
        logger.error(f"Error scraping URL {url}: {str(e)}")
        return {
            "success": False,
            "url": url,
            "error": str(e)
        }