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
  message = '';
  documentList: documentlist[] = [];




  constructor(private api: DocqueryApiService, public documentState: DocumentStateService) { }

  ngOnInit(): void {
    this.api.getDocumentList().subscribe({
      next: documentList => {
        this.documentList = documentList;
      }
    });
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;

    if (input.files && input.files.length > 0) {
      this.selectedFile = input.files[0];
      this.message = '';
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

        this.message =
          `${response.fileName} uploaded successfully. ` +
          `${response.chunkCount} chunks indexed.` +
          `${response.documentId} Document ID.`;
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
}