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
