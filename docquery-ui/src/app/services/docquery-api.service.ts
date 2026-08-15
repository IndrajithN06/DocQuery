import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class DocqueryApiService {

  private readonly apiUrl = 'https://localhost:7095/api';

  constructor(private http: HttpClient) { }

  uploadDocument(file: File): Observable<any> {
    const formData = new FormData();

    formData.append('file', file);

    return this.http.post(
      `${this.apiUrl}/Document/upload`,
      formData
    );
  }

  askQuestion(question: string): Observable<any> {
    return this.http.post(
      `${this.apiUrl}/Rag/ask`,
      {
        question
      }
    );
  }
}