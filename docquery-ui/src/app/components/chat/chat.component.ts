import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { DocqueryApiService } from '../../services/docquery-api.service';

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
  asking = false;

  constructor(private api: DocqueryApiService) { }

  ask(): void {
    if (!this.question.trim()) {
      return;
    }

    this.asking = true;
    this.answer = '';

    this.api.askQuestion(this.question).subscribe({
      next: response => {
        this.answer = response.answer;
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