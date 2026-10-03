import { Component } from '@angular/core';
import { DocqueryApiService } from '../../services/docquery-api.service';
import { documentlist } from '../../models/documentlist.model';
import { DocumentStateService } from '../../services/document-state.service';

@Component({
  selector: 'app-document-upload',
  standalone: true,
  templateUrl: './document-upload.component.html',
  styleUrl: './document-upload.component.css'
})
export class DocumentUploadComponent {

  selectedFile: File | null = null;
  uploading = false;
  deletingDocumentId: string | null = null;
  dragActive = false;
  message = '';




  constructor(private api: DocqueryApiService, public documentState: DocumentStateService) { }

  ngOnInit(): void {
    this.refreshDocuments();
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;

    if (input.files && input.files.length > 0) {
      this.selectedFile = input.files[0];
      this.message = '';
    }
  }

  onDragOver(event: DragEvent): void {
    event.preventDefault();
    this.dragActive = true;
  }

  onDragLeave(): void {
    this.dragActive = false;
  }

  onDrop(event: DragEvent): void {
    event.preventDefault();
    this.dragActive = false;
    const file = event.dataTransfer?.files.item(0);

    if (file?.type === 'application/pdf' || file?.name.toLowerCase().endsWith('.pdf')) {
      this.selectedFile = file;
      this.message = '';
    } else if (file) {
      this.message = 'Please choose a PDF file.';
    }
  }

  upload(): void {
    if (!this.selectedFile) {
      this.message = 'Please select a PDF first.';
      return;
    }

    this.uploading = true;
    this.message = '';

    this.api.uploadDocument(this.selectedFile).subscribe({
      next: response => {
        this.uploading = false;
        this.selectedFile = null;

        this.message =
          `${response.fileName} uploaded successfully. ` +
          `${response.chunkCount} chunks indexed.` +
          `${response.documentId} Document ID.`;
        this.refreshDocuments();
      },
      error: error => {
        this.uploading = false;
        console.error(error);

        this.message = 'Upload failed.';
      }
    });
  }

  selectDocument(document: documentlist): void {
    this.documentState.selectedDocumentId.set(document.documentId);

  }

  deleteDocument(document: documentlist): void {
    if (!window.confirm(`Delete "${document.documentName}"?`)) {
      return;
    }

    this.deletingDocumentId = document.documentId;
    this.message = '';

    this.api.deleteDocument(document.documentId).subscribe({
      next: () => {
        this.deletingDocumentId = null;
        this.message = `${document.documentName} deleted.`;
        this.refreshDocuments();
      },
      error: error => {
        this.deletingDocumentId = null;
        console.error(error);
        this.message = 'Delete failed.';
      }
    });
  }

  private refreshDocuments(): void {
    this.documentState.refreshDocuments(this.api).subscribe({
      error: error => {
        console.error(error);
        this.message = 'Unable to load documents.';
      }
    });
  }
}
