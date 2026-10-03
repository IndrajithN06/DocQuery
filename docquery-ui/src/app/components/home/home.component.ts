import { Component } from '@angular/core';
import { DocumentUploadComponent } from '../document-upload/document-upload.component';
import { ChatComponent } from '../chat/chat.component';
import { Router, RouterLink } from '@angular/router';
import { SupabaseService } from '../../services/auth-services/supabase.service';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [
    DocumentUploadComponent,
    ChatComponent,
    RouterLink
  ],
  templateUrl: './home.component.html',
  styleUrl: './home.component.css'
})
export class HomeComponent {

  constructor(
    private supabaseService: SupabaseService,
    private router: Router
  ) { }

  async logout() {
    const { error } = await this.supabaseService.signOut();

    if (error) {
      console.error('Logout failed:', error);
      return;
    }

    await this.router.navigate(['/login']);
  }
}
