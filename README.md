# DocQuery

DocQuery is an AI-powered document question-answering application built around a Retrieval-Augmented Generation (RAG) pipeline. Users authenticate with Supabase, upload PDF documents, select a document, and ask questions whose answers are grounded in the document's retrieved content.

The current application consists of:

- An Angular 19 standalone frontend.
- An ASP.NET Core .NET 8 Web API.
- Supabase Auth for signup, login, logout, and access JWTs.
- Qdrant for vector storage, payload metadata, filtering, and similarity search.
- PdfPig for PDF text extraction.
- Ollama for local development inference.
- Gemini hosted chat and embedding inference outside the Development environment.

## Architecture

```text
Angular UI
   │
   ├── Supabase Auth session
   │       └── access JWT
   │
   └── Angular HTTP interceptor
           └── Authorization: Bearer <access_token>
                    │
                    ▼
          ASP.NET Core .NET 8 API
                    │
          JWT validation + [Authorize]
                    │
          authenticated Supabase user ID
                    │
          ┌─────────┴──────────┐
          │                    │
   PDF extraction        RAG question
   and chunking                 │
          │             verify document ownership
          ▼                    │
    embeddings                  │
          │                    │
          └─────────┬──────────┘
                    ▼
                  Qdrant
       vectors + document/chunk payloads
       filtered by userId/documentId
                    │
                    ▼
           relevant document chunks
                    │
                    ▼
          Ollama or hosted Gemini LLM
                    │
                    ▼
                Angular UI
```

### Authentication flow

```text
Signup / login with Supabase
        ↓
Supabase access JWT
        ↓
Angular Supabase HTTP interceptor
        ↓
Authorization: Bearer <access_token>
        ↓
ASP.NET Core JwtBearer validation
        ↓
sub claim → current authenticated user ID
        ↓
user-specific document and Qdrant access
```

The backend never accepts a frontend-supplied `userId` for authorization. It derives the current user from the validated JWT `sub` claim through `ICurrentUserService`.

## Features

- Supabase email/password signup, login, and logout.
- Angular standalone application with functional HTTP interceptor registration.
- ASP.NET Core JWT validation using the Supabase issuer and public signing-key discovery.
- Protected API controllers using `[Authorize]`.
- PDF upload with text extraction through PdfPig.
- Page-aware text chunking with a 1,000-character chunk size and 200-character overlap.
- Embedding generation for document chunks and questions.
- Qdrant vector similarity search with a top-three result limit for RAG.
- User-specific document ownership using the authenticated Supabase user ID.
- `userId` stored on every newly-created Qdrant chunk payload.
- Document listing restricted to the signed-in user.
- Ownership checks before RAG queries and document deletion.
- Document deletion that removes matching Qdrant chunks using both `userId` and `documentId`.
- Automatic document-list refresh after upload and deletion.
- Selected-document state shared between the document library and chat UI.
- Responsive Angular UI with login, signup, upload, document library, deletion, empty states, loading states, and source display.

Existing Qdrant chunks without a `userId` payload are intentionally excluded from user-specific listing, search, and deletion operations. This prevents legacy data from being assigned to the wrong user; such documents need to be re-uploaded or migrated separately if they should become available.

## RAG pipeline

### Document ingestion

1. The authenticated Angular user selects or drops a PDF.
2. `DocumentController` obtains the authenticated user ID from the JWT.
3. `PdfService` extracts text page by page.
4. `TextChunker` creates overlapping chunks while preserving page numbers.
5. The configured `ILlmService` generates a 768-dimensional embedding for each chunk.
6. `QdrantService` stores each vector with `text`, `document`, `pageNumber`, `documentId`, and `userId` payload fields.
7. The Angular document state refreshes and displays the new document.

### Question answering

1. The user selects one of their documents in the Angular UI.
2. The user submits a question to `/api/Rag/ask`.
3. The API obtains the authenticated user ID from `HttpContext.User` through `ICurrentUserService`.
4. If a document ID is supplied, the API verifies that the document belongs to that user.
5. The question is embedded using the configured LLM provider.
6. Qdrant search is filtered by the authenticated `userId` and selected `documentId`.
7. The top relevant chunks are assembled into an instruction prompt.
8. The LLM generates a grounded answer.
9. The API returns the answer and distinct document/page sources, which Angular displays.

## Security and authorization

- Supabase manages user authentication and issues access JWTs.
- The Angular interceptor attaches the access token only to requests matching the configured API origin and `/api` path.
- ASP.NET Core uses `AddJwtBearer` with the Supabase authority and validates issuer, audience, lifetime, and signing key.
- Document, RAG, chat, embedding, and Qdrant controllers are protected with `[Authorize]`.
- The authenticated user ID is derived from the validated JWT, never from request data supplied by Angular.
- Uploads write the authenticated user ID into every Qdrant chunk.
- Lists and searches apply a `userId` payload filter.
- RAG checks document ownership before retrieving document-specific chunks.
- Deletes require a matching authenticated `userId` and `documentId` filter.
- The Supabase service-role/secret key is not used in the Angular frontend.
- Do not log or commit access tokens, API keys, private keys, or other credentials.

## API endpoints

All endpoints are under `/api` and require an authenticated bearer token.

| Endpoint | Purpose |
| --- | --- |
| `POST /api/Document/upload` | Upload and index a PDF for the current user. |
| `GET /api/Document/list-documents` | List documents belonging to the current user. |
| `DELETE /api/Document/{documentId}` | Delete an owned document and its Qdrant chunks. |
| `POST /api/Rag/ask` | Ask a question against the selected document. |
| `POST /api/Chat` | Generate a general authenticated chat response. |
| `POST /api/Embedding` | Generate an embedding through the configured LLM service. |
| `POST /api/Qdrant/create-collection` | Ensure the Qdrant collection and payload indexes exist. |
| `POST /api/Qdrant/search` | Perform an authenticated user-scoped vector search. |

Swagger/OpenAPI is enabled when the API runs in Development.

## Technology stack

| Technology | Current usage |
| --- | --- |
| Angular | Angular 19 standalone frontend. |
| TypeScript | Angular application implementation. |
| ASP.NET Core | .NET 8 Web API backend. |
| C# | API, services, document processing, and RAG implementation. |
| Supabase Auth | Signup, login, logout, sessions, and JWT issuance. |
| Qdrant.Client `1.19.0` | Qdrant collection, vector, payload, filter, and delete operations. |
| PdfPig `0.1.15` | PDF text extraction. |
| Semantic Kernel `1.79.0` | Backend LLM-related dependency. |
| Semantic Kernel Ollama connector `1.79.0-alpha` | Ollama integration dependency. |
| Ollama | Development chat and embedding inference. |
| Gemini | Non-Development hosted chat and embedding inference. |
| Docker | Local Qdrant and containerized API workflows. |
| RxJS | Angular HTTP and document-state observables. |

SQL Server is not currently used by the repository. Document metadata is derived from Qdrant payloads; there is no separate relational document database or SQL migration in the current implementation.

## Project structure

```text
DocQuery/
├── DocQuery.sln
├── DocQuery/
│   ├── Controllers/
│   │   ├── ChatController.cs
│   │   ├── DocumentController.cs
│   │   ├── EmbeddingController.cs
│   │   ├── QdrantController.cs
│   │   └── RagController.cs
│   ├── Models/
│   │   ├── DocumentChunk.cs
│   │   ├── DocumentList.cs
│   │   ├── PdfPageContent.cs
│   │   ├── RagResponse.cs
│   │   └── SearchResult.cs
│   ├── Services/
│   │   ├── CurrentUserService.cs
│   │   ├── GeminiService.cs
│   │   ├── ICurrentUserService.cs
│   │   ├── ILlmService.cs
│   │   ├── OllamaService.cs
│   │   ├── PdfService.cs
│   │   ├── QdrantService.cs
│   │   ├── RagService.cs
│   │   └── TextChunker.cs
│   ├── Program.cs
│   ├── DocQuery.csproj
│   ├── Dockerfile
│   ├── appsettings.json
│   └── appsettings.Development.json
└── docquery-ui/
    ├── src/app/
    │   ├── components/
    │   │   ├── chat/
    │   │   ├── document-upload/
    │   │   ├── home/
    │   │   ├── login/
    │   │   └── signup/
    │   ├── interceptors/
    │   │   └── supabase-auth.interceptor.ts
    │   ├── services/
    │   │   ├── auth-services/supabase.service.ts
    │   │   ├── docquery-api.service.ts
    │   │   └── document-state.service.ts
    │   ├── app.config.ts
    │   └── app.routes.ts
    ├── environment.ts
    ├── environment.development.ts
    ├── angular.json
    └── package.json
```

## Local development

### Prerequisites

- .NET 8 SDK.
- Node.js and npm.
- Docker Desktop.
- Angular dependencies installed from `docquery-ui/package.json`.
- Ollama for the Development LLM path.

### Run Qdrant locally

The Qdrant client uses `localhost` and gRPC port `6334` by default. Start Qdrant with both its REST and gRPC ports available:

```bash
docker run -d \
  --name qdrant \
  -p 6333:6333 \
  -p 6334:6334 \
  qdrant/qdrant
```

The API creates the `docquery_documents` collection if it does not exist and ensures keyword indexes for `documentId` and `userId`.

### Run Ollama for Development

Development uses `OllamaService` with:

- Chat model: `qwen2.5:3b`.
- Embedding model: `nomic-embed-text`.
- Base URL default: `http://localhost:11434/`.

Make sure Ollama is running and the models are available:

```bash
ollama pull qwen2.5:3b
ollama pull nomic-embed-text
```

### Run the API

From the repository root:

```bash
dotnet restore DocQuery/DocQuery.csproj
dotnet run --project DocQuery --launch-profile https
```

The configured Development launch profile exposes:

- HTTPS: `https://localhost:7095`
- HTTP: `http://localhost:5006`
- Swagger: `/swagger`

The Angular Development environment targets `https://localhost:7095/api`.

### Run the Angular frontend

From `docquery-ui`:

```bash
npm install
npm start
```

The Angular development server runs at `http://localhost:4200`.

## Configuration and environment variables

The repository contains public client configuration in:

- `docquery-ui/environment.development.ts` for local development.
- `docquery-ui/environment.ts` for production builds.

These values include the API URL, Supabase project URL, and Supabase publishable key. Publishable Supabase client configuration is expected in the browser, but it must not be confused with a service-role or secret key.

The API reads the following configuration keys:

| Key | Required usage |
| --- | --- |
| `Supabase:Url` | Supabase project URL used for JWT authority validation. |
| `Supabase:JwtAudience` | JWT audience; defaults to `authenticated`. |
| `Qdrant:Host` | Qdrant host; defaults to `localhost`. |
| `Qdrant:ApiKey` | Optional Qdrant API key for a secured Qdrant deployment. |
| `Qdrant:UseHttps` | Whether the Qdrant client should use HTTPS. |
| `Ollama:BaseUrl` | Optional Development Ollama base URL; defaults to localhost. |
| `Gemini:ApiKey` | Required outside Development; must be supplied as a secret. |
| `Gemini:ChatModel` | Optional hosted Gemini chat model override. |
| `Gemini:EmbeddingModel` | Optional hosted Gemini embedding model override. |
| `Cors:Origins` | Optional JSON array of allowed browser origins. |

For local or hosted environments, prefer environment variables or a secret manager for private values. Never commit Gemini API keys, Qdrant private keys, database credentials, service-role keys, access tokens, private keys, or `.env` files containing secrets.

## Deployment

The repository currently supports this deployment shape:

- The ASP.NET Core API can be built with `DocQuery/Dockerfile`. The container listens on the platform-provided `PORT` value and defaults to port `8080`.
- The production Angular environment points its API base URL at `https://docquery-api.onrender.com/api`, indicating the intended/current hosted API endpoint.
- Supabase provides the hosted authentication project and JWT issuer.
- Qdrant is configurable through `Qdrant:Host`, `Qdrant:ApiKey`, and `Qdrant:UseHttps`, so production vector storage can be a secured hosted Qdrant deployment or another reachable Qdrant instance.
- No frontend hosting configuration is committed, so the repository does not establish a specific Angular hosting provider.
- In non-Development API environments, the configured LLM implementation is `GeminiService` and requires `Gemini:ApiKey` through secure deployment configuration.

The repository does not contain a production Qdrant provider declaration, a frontend hosting workflow, or an infrastructure-as-code deployment definition. Those should be configured by the deployment environment rather than inferred from the application source.

## Future improvements

The following are not currently implemented and are possible next steps:

- Add automated backend integration tests for JWT ownership, cross-user access, RAG authorization, and deletion.
- Add frontend tests for upload refresh, deletion refresh, selection clearing, and interceptor behavior.
- Add background processing and progress reporting for large PDFs.
- Add upload size limits, rate limiting, and more formal API error responses.
- Add streaming LLM responses.
- Add retrieval evaluation, relevance metrics, and a regression dataset.
- Add hybrid keyword/vector retrieval or reranking.
- Add observability, structured logs, tracing, and operational health checks.
- Add conversation history and multi-turn chat state.
- Add an explicit production Qdrant deployment configuration and backup/retention strategy.
- Add a frontend deployment workflow and environment substitution strategy.

## Learning goals

DocQuery demonstrates the practical building blocks of a secured RAG application:

```text
Authenticated user
        ↓
PDF ingestion
        ↓
Text extraction and chunking
        ↓
Embeddings
        ↓
User-scoped vector storage
        ↓
Filtered semantic retrieval
        ↓
Context construction
        ↓
LLM generation
        ↓
Grounded answer and sources
```

DocQuery is a learning and engineering project. Additional testing, evaluation, observability, upload controls, and deployment hardening would be appropriate before treating it as an enterprise document platform.
