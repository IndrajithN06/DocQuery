import { Component } from '@angular/core';
import { DocqueryApiService } from '../../services/docquery-api.service';

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

  constructor(private api: DocqueryApiService) { }

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
          `${response.chunkCount} chunks indexed.`;
      },
      error: error => {
        this.uploading = false;
        console.error(error);

        this.message = 'Upload failed.';
      }
    });
  }
}