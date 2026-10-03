import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { SupabaseService } from '../../services/auth-services/supabase.service';

@Component({
  selector: 'app-signup',
  standalone: true,
  imports: [FormsModule, RouterLink],
  templateUrl: './signup.component.html',
  styleUrl: './signup.component.css'
})
export class SignupComponent {

  email = '';
  password = '';
  confirmPassword = '';

  loading = false;
  errorMessage = '';
  successMessage = '';

  constructor(
    private supabaseService: SupabaseService,
    private router: Router
  ) { }

  async signup() {

    this.errorMessage = '';
    this.successMessage = '';

    if (!this.email || !this.password || !this.confirmPassword) {
      this.errorMessage = 'Please fill in all fields.';
      return;
    }

    if (this.password !== this.confirmPassword) {
      this.errorMessage = 'Passwords do not match.';
      return;
    }

    if (this.password.length < 6) {
      this.errorMessage = 'Password must be at least 6 characters.';
      return;
    }

    this.loading = true;

    try {

      const { data, error } =
        await this.supabaseService.signUp(
          this.email,
          this.password
        );

      if (error) {
        this.errorMessage = error.message;
        return;
      }

      if (data.user) {

        if (data.session) {
          // Email confirmation is disabled
          await this.router.navigate(['/chat']);
        } else {
          // Email confirmation is enabled
          this.successMessage =
            'Account created successfully. Please check your email to verify your account.';
        }
      }

    } catch (error) {

      this.errorMessage =
        'Something went wrong. Please try again.';

    } finally {

      this.loading = false;

    }
  }
}