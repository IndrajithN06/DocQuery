import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ChatResponse } from '../models/chat-response.model';
import { documentlist } from '../models/documentlist.model';
import { environment } from '../../../environment';

@Injectable({
  providedIn: 'root'
})
export class DocqueryApiService {

  private readonly apiUrl = environment.apiUrl;

  constructor(private http: HttpClient) { }

  uploadDocument(file: File): Observable<any> {
    const formData = new FormData();

    formData.append('file', file);

    return this.http.post(
      `${this.apiUrl}/Document/upload`,
      formData
    );
  }

  getDocumentList(): Observable<documentlist[]> {
    return this.http.get<documentlist[]>(
      `${this.apiUrl}/Document/list-documents`
    );
  }

  deleteDocument(documentId: string): Observable<void> {
    return this.http.delete<void>(
      `${this.apiUrl}/Document/${encodeURIComponent(documentId)}`
    );
  }

  askQuestion(question: string, documentId: string | null): Observable<ChatResponse> {
    return this.http.post<ChatResponse>(
      `${this.apiUrl}/Rag/ask`,
      {
        question, documentId
      }
    );
  }
}
