import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { DocqueryApiService } from '../../services/docquery-api.service';
import { ChatSource } from '../../models/chat-response.model';
import { ChatResponse } from '../../models/chat-response.model';
import { DocumentStateService } from '../../services/document-state.service';

@Component({
  selector: 'app-chat',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './chat.component.html',
  styleUrl: './chat.component.css'
})
export class ChatComponent {

  question = '';
  answer = '';
  sources: ChatSource[] = [];
  asking = false;

  constructor(private api: DocqueryApiService, private documentState: DocumentStateService) { }

  ask(): void {
    if (!this.question.trim()) {
      return;
    }

    this.asking = true;
    this.answer = '';
    const selectedDocumentId = this.documentState.selectedDocumentId();

    this.api.askQuestion(this.question, selectedDocumentId).subscribe({
      next: response => {
        this.answer = response.answer;
        this.sources = response.sources;
        this.asking = false;
      },
      error: error => {
        console.error(error);
        this.answer = 'Something went wrong while processing your question.';
        this.asking = false;
      }
    });
  }
}