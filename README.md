# DocQuery

DocQuery is a local Retrieval-Augmented Generation (RAG) application that allows users to upload documents and ask natural-language questions about their content.

The application combines document processing, embeddings, vector search, and a local LLM to retrieve relevant document chunks and generate grounded answers.

## Architecture

```text
                         ┌──────────────────────┐
                         │      Angular UI      │
                         │                      │
                         │ Document Upload      │
                         │ Question / Chat      │
                         └──────────┬───────────┘
                                    │
                                    ▼
                         ┌──────────────────────┐
                         │   ASP.NET Core API   │
                         │                      │
                         │ Document Endpoint    │
                         │ RAG Endpoint         │
                         └──────────┬───────────┘
                                    │
                    ┌───────────────┴───────────────┐
                    │                               │
                    ▼                               ▼
          ┌──────────────────┐            ┌──────────────────┐
          │ Document         │            │      Ollama      │
          │ Processing       │            │                  │
          │                  │            │ Embeddings       │
          │ Extract text     │            │ + Local LLM      │
          │ Chunk document   │            └────────┬─────────┘
          └────────┬─────────┘                     │
                   │                               │
                   ▼                               ▼
          ┌──────────────────┐            ┌──────────────────┐
          │    Embedding     │───────────▶│     Qdrant       │
          │                  │            │                  │
          │ Text → Vector    │            │ Vector Storage   │
          └──────────────────┘            │ Semantic Search  │
                                          └────────┬─────────┘
                                                   │
                                                   ▼
                                          Relevant Chunks
                                                   │
                                                   ▼
                                          ┌──────────────────┐
                                          │   RAG Service    │
                                          │                  │
                                          │ Build context    │
                                          │ Create prompt    │
                                          │ Generate answer  │
                                          └────────┬─────────┘
                                                   │
                                                   ▼
                                            Angular UI
```

## RAG Flow

### Document ingestion

```text
Document Upload
      ↓
Extract Text
      ↓
Split into Chunks
      ↓
Generate Embeddings
      ↓
Store Vectors
      ↓
Qdrant Collection
```

### Question answering

```text
User Question
      ↓
Generate Query Embedding
      ↓
Qdrant Semantic Search
      ↓
Retrieve Relevant Chunks
      ↓
Build Context
      ↓
Create RAG Prompt
      ↓
Ollama LLM
      ↓
Grounded Answer
```

## Technologies

| Technology           | Purpose                               |
| -------------------- | ------------------------------------- |
| ASP.NET Core Web API | Backend REST API                      |
| C# / .NET            | Application implementation            |
| Angular              | Frontend UI                           |
| Qdrant               | Vector database                       |
| Ollama               | Local LLM and embedding inference     |
| Semantic Search      | Retrieval of relevant document chunks |
| Docker               | Running Qdrant locally                |

## Main Components

### Angular Frontend

The frontend provides:

* Document upload
* Chat/question interface
* Display of generated answers
* Communication with the ASP.NET Core API

### ASP.NET Core API

The backend exposes endpoints for:

* Uploading documents
* Processing document content
* Performing semantic search
* Asking questions using the RAG pipeline

### Ollama Service

Ollama is used locally for:

* Generating embeddings
* Generating natural-language answers

This allows the project to run without depending on a hosted LLM API.

### Qdrant Service

Qdrant stores document embeddings and performs vector similarity search.

The application:

1. Generates an embedding for each document chunk.
2. Stores the embedding and associated text metadata in Qdrant.
3. Generates an embedding for a user question.
4. Searches for the most similar chunks.
5. Uses those chunks as context for the LLM.

## Project Structure

```text
DocQuery/
│
├── Backend/
│   └── ...
│
├── Frontend/
│   └── ...
│
├── docker/
│   └── ...
│
├── README.md
└── .gitignore
```

> The exact folder names may vary depending on the current repository structure.

## RAG Service Flow

The core RAG pipeline follows this pattern:

```text
Question
   ↓
Ollama Embedding
   ↓
Qdrant Search
   ↓
Top K Relevant Chunks
   ↓
Context Construction
   ↓
RAG Prompt
   ↓
Ollama LLM
   ↓
Answer
```

For example:

```text
Question:
"What technologies are listed in my resume?"

        ↓

Query embedding generated

        ↓

Qdrant searches document vectors

        ↓

Top relevant resume chunks retrieved

        ↓

Chunks become LLM context

        ↓

Ollama generates the answer
```

## Local Development

### Prerequisites

Install:

* .NET SDK
* Node.js
* Angular CLI
* Docker Desktop
* Ollama

### Run Qdrant

Example local Docker setup:

```bash
docker run -d \
  --name qdrant \
  -p 6333:6333 \
  -p 6334:6334 \
  qdrant/qdrant
```

Qdrant is then available locally.

### Start Ollama

Verify Ollama is running:

```bash
ollama list
```

The application can use a local chat model and embedding model configured in the project.

### Run the ASP.NET Core API

From the backend directory:

```bash
dotnet restore
dotnet run
```

The API can then be accessed using Swagger if enabled.

### Run Angular

From the frontend directory:

```bash
npm install
ng serve
```

The Angular application is typically available at:

```text
http://localhost:4200
```

## Example Usage

### 1. Upload a document

Upload a document through the Angular interface.

```text
Document
   ↓
Text extraction
   ↓
Chunking
   ↓
Embedding generation
   ↓
Qdrant
```

### 2. Ask a question

Example:

```text
What technologies are mentioned in the uploaded resume?
```

The application retrieves the most relevant chunks and sends them to the local LLM.

Example response:

```text
The resume mentions ASP.NET Core, Angular, SQL Server,
Entity Framework Core, Docker, and GitHub Actions.
```

## Key RAG Concepts Demonstrated

This project demonstrates the core building blocks of a RAG application:

* Document ingestion
* Text chunking
* Embedding generation
* Vector databases
* Semantic similarity search
* Top-K retrieval
* Context construction
* Prompt augmentation
* Local LLM inference
* Retrieval-Augmented Generation

## Why RAG?

Instead of sending an entire document directly to the LLM, DocQuery retrieves only the most relevant sections.

```text
Traditional LLM
Document → LLM → Answer

DocQuery
Document
   ↓
Chunks
   ↓
Embeddings
   ↓
Vector Search
   ↓
Relevant Chunks
   ↓
LLM
   ↓
Answer
```

This reduces unnecessary context and allows the application to work with documents that are larger than the model's useful context for a particular question.

## Current Capabilities

* Local document question answering
* Document upload through Angular
* Embedding generation using Ollama
* Vector storage using Qdrant
* Semantic search
* RAG-based answer generation
* Local development without a hosted LLM API
* ASP.NET Core backend
* Angular frontend

## Future Improvements

Potential improvements include:

* Hybrid keyword + vector retrieval
* Better document chunking strategies
* Metadata filtering
* Re-ranking retrieved chunks
* Citation/source references in answers
* Retrieval evaluation
* RAG evaluation dataset
* Prompt-injection protection
* Conversation memory
* Streaming LLM responses
* Authentication and authorization
* Production deployment using Azure or containers
* Observability and request tracing

## Learning Goals

The project was built to understand the practical implementation of RAG systems rather than only the theory.

The main learning objectives are:

```text
Documents
   ↓
Embeddings
   ↓
Vector Database
   ↓
Semantic Retrieval
   ↓
Context Construction
   ↓
LLM
   ↓
Grounded Answer
```

## Disclaimer

DocQuery is a learning and engineering prototype demonstrating a local Retrieval-Augmented Generation architecture. It is not intended to be treated as a production-grade document management or enterprise knowledge platform without additional security, evaluation, observability, and operational controls.
