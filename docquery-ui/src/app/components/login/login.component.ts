import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { SupabaseService } from '../../services/auth-services/supabase.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [FormsModule, RouterLink],
  templateUrl: './login.component.html',
  styleUrl: './login.component.css'
})
export class LoginComponent {

  email = '';
  password = '';

  loading = false;
  errorMessage = '';

  constructor(
    private supabaseService: SupabaseService,
    private router: Router
  ) { }

  async login() {

    this.errorMessage = '';

    if (!this.email || !this.password) {
      this.errorMessage = 'Please enter email and password.';
      return;
    }

    this.loading = true;

    try {

      const { data, error } =
        await this.supabaseService.signIn(
          this.email,
          this.password
        );

      if (error) {
        this.errorMessage = error.message;
        return;
      }

      if (data.session) {

        await this.router.navigate(['/chat']);

      }

    } catch (error) {

      this.errorMessage =
        'Something went wrong. Please try again.';

    } finally {

      this.loading = false;

    }
  }
}