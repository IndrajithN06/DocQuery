import { Injectable, signal } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class DocumentStateService {
  selectedDocumentId = signal<string | null>(null);
  constructor() { }

}
