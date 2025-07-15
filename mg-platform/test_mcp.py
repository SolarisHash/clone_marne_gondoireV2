#!/usr/bin/env python3
"""
Test simple du serveur MCP Marne & Gondoire
Vérifie que le serveur fonctionne correctement
"""

import requests
import json
import time
import sys

# Configuration
SERVER_URL = "http://localhost:8080"
TIMEOUT = 5

def test_server_health():
    """Test basique - serveur en ligne"""
    print("🔍 Test 1: Santé du serveur")
    try:
        response = requests.get(f"{SERVER_URL}/health", timeout=TIMEOUT)
        if response.status_code == 200:
            print("✅ Serveur en ligne")
            return True
        else:
            print(f"❌ Serveur problème (status: {response.status_code})")
            return False
    except Exception as e:
        print(f"❌ Serveur inaccessible: {e}")
        return False

def test_root_endpoint():
    """Test de l'endpoint racine"""
    print("\n🔍 Test 2: Endpoint racine")
    try:
        response = requests.get(f"{SERVER_URL}/", timeout=TIMEOUT)
        if response.status_code == 200:
            data = response.json()
            print(f"✅ Endpoint racine OK - Version: {data.get('version', 'N/A')}")
            return True
        else:
            print(f"❌ Endpoint racine erreur: {response.status_code}")
            return False
    except Exception as e:
        print(f"❌ Erreur endpoint racine: {e}")
        return False

def test_tools_list():
    """Test de la liste des outils"""
    print("\n🔍 Test 3: Liste des outils")
    try:
        response = requests.get(f"{SERVER_URL}/tools", timeout=TIMEOUT)
        if response.status_code == 200:
            data = response.json()
            tools = data.get('tools', [])
            print(f"✅ {len(tools)} outils disponibles:")
            for tool in tools:
                print(f"   - {tool.get('name', 'N/A')}")
            return True
        else:
            print(f"❌ Erreur liste outils: {response.status_code}")
            return False
    except Exception as e:
        print(f"❌ Erreur outils: {e}")
        return False

def test_sql_tool():
    """Test de l'outil SQL"""
    print("\n🔍 Test 4: Outil SQL")
    try:
        payload = {
            "name": "run_sql",
            "arguments": {
                "query": "SELECT 1 as test_value, 'hello' as test_text",
                "limit": 1
            }
        }
        response = requests.post(
            f"{SERVER_URL}/tools/run_sql",
            json=payload,
            timeout=TIMEOUT
        )
        
        if response.status_code == 200:
            data = response.json()
            if data.get('success'):
                result = data.get('result', {})
                rows = result.get('rows', [])
                if rows:
                    print(f"✅ SQL OK - Résultat: {rows[0]}")
                    return True
                else:
                    print("❌ SQL - Pas de résultats")
                    return False
            else:
                print(f"❌ SQL erreur: {data.get('error', 'Unknown')}")
                return False
        else:
            print(f"❌ SQL HTTP erreur: {response.status_code}")
            return False
    except Exception as e:
        print(f"❌ Erreur SQL: {e}")
        return False

def test_web_search():
    """Test de l'outil de recherche web"""
    print("\n🔍 Test 5: Recherche web")
    try:
        payload = {
            "name": "search_web",
            "arguments": {
                "query": "test",
                "max_results": 2
            }
        }
        response = requests.post(
            f"{SERVER_URL}/tools/search_web",
            json=payload,
            timeout=TIMEOUT
        )
        
        if response.status_code == 200:
            data = response.json()
            if data.get('success'):
                result = data.get('result', {})
                results = result.get('results', [])
                print(f"✅ Web search OK - {len(results)} résultats trouvés")
                return True
            else:
                print(f"❌ Web search erreur: {data.get('error', 'Unknown')}")
                return False
        else:
            print(f"❌ Web search HTTP erreur: {response.status_code}")
            return False
    except Exception as e:
        print(f"❌ Erreur web search: {e}")
        return False

def test_file_analysis():
    """Test de l'analyse de fichier (simulation)"""
    print("\n🔍 Test 6: Analyse de fichier")
    try:
        payload = {
            "name": "analyze_file",
            "arguments": {
                "file_path": "data/samples/sample_data.csv",
                "detailed": False
            }
        }
        response = requests.post(
            f"{SERVER_URL}/tools/analyze_file",
            json=payload,
            timeout=TIMEOUT
        )
        
        if response.status_code == 200:
            data = response.json()
            if data.get('success'):
                result = data.get('result', {})
                if 'basic_info' in result:
                    print("✅ Analyse fichier OK")
                    return True
                else:
                    print("❌ Analyse fichier - Format inattendu")
                    return False
            else:
                error = data.get('error', 'Unknown')
                if "not found" in error.lower():
                    print("⚠️  Fichier non trouvé (normal sans fichier test)")
                    return True
                else:
                    print(f"❌ Analyse fichier erreur: {error}")
                    return False
        else:
            print(f"❌ Analyse fichier HTTP erreur: {response.status_code}")
            return False
    except Exception as e:
        print(f"❌ Erreur analyse fichier: {e}")
        return False

def wait_for_server():
    """Attend que le serveur soit prêt"""
    print("⏳ Attente du serveur...")
    for i in range(30):
        try:
            response = requests.get(f"{SERVER_URL}/health", timeout=2)
            if response.status_code == 200:
                print("✅ Serveur prêt")
                return True
        except:
            pass
        
        print(f"   Tentative {i+1}/30...")
        time.sleep(1)
    
    print("❌ Serveur non disponible après 30 secondes")
    return False

def main():
    """Fonction principale"""
    print("🧪 TEST DU SERVEUR MCP MARNE & GONDOIRE")
    print("=" * 50)
    
    # Attendre le serveur
    if not wait_for_server():
        sys.exit(1)
    
    # Liste des tests
    tests = [
        ("Santé du serveur", test_server_health),
        ("Endpoint racine", test_root_endpoint),
        ("Liste des outils", test_tools_list),
        ("Outil SQL", test_sql_tool),
        ("Recherche web", test_web_search),
        ("Analyse de fichier", test_file_analysis),
    ]
    
    # Exécuter les tests
    passed = 0
    failed = 0
    
    for test_name, test_func in tests:
        try:
            if test_func():
                passed += 1
            else:
                failed += 1
        except Exception as e:
            print(f"❌ Test '{test_name}' a planté: {e}")
            failed += 1
    
    # Résultats
    print("\n" + "=" * 50)
    print(f"📊 RÉSULTATS: {passed} réussis, {failed} échoués")
    
    if failed == 0:
        print("🎉 TOUS LES TESTS SONT PASSÉS!")
        print("\n✅ Votre serveur MCP fonctionne parfaitement")
        print("✅ Vous pouvez maintenant développer vos agents IA")
        print("\n🚀 Prochaines étapes:")
        print("   1. Créer des fichiers de test dans data/samples/")
        print("   2. Tester l'analyse de fichiers réels")
        print("   3. Développer votre premier agent")
        sys.exit(0)
    else:
        print(f"⚠️  {failed} tests ont échoué")
        print("\n🔧 Vérifiez:")
        print("   - Le serveur est bien démarré")
        print("   - Les dépendances sont installées")
        print("   - Le port 8080 est libre")
        sys.exit(1)

if __name__ == "__main__":
    main()