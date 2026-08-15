import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { DocumentUploadComponent } from './components/document-upload/document-upload.component';
import { ChatComponent } from './components/chat/chat.component';
@Component({
  selector: 'app-root',
  standalone: true,
  imports: [DocumentUploadComponent,
    ChatComponent],
  templateUrl: './app.component.html',
  styleUrl: './app.component.css'
})
export class AppComponent {

}
