import { Injectable, signal } from '@angular/core';
import { Observable, finalize, tap } from 'rxjs';
import { documentlist } from '../models/documentlist.model';
import { DocqueryApiService } from './docquery-api.service';

@Injectable({
  providedIn: 'root'
})
export class DocumentStateService {
  selectedDocumentId = signal<string | null>(null);
  documents = signal<documentlist[]>([]);
  loading = signal(false);

  constructor() { }

  refreshDocuments(api: DocqueryApiService): Observable<documentlist[]> {
    this.loading.set(true);

    return api.getDocumentList().pipe(
      tap(documents => this.setDocuments(documents)),
      finalize(() => this.loading.set(false))
    );
  }

  setDocuments(documents: documentlist[]): void {
    this.documents.set(documents);

    const selectedId = this.selectedDocumentId();
    if (selectedId && !documents.some(document => document.documentId === selectedId)) {
      this.selectedDocumentId.set(null);
    }
  }

}
