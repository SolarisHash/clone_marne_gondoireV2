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
