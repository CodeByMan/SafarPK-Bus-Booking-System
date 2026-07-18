import { CommonModule } from '@angular/common';
import { Component, HostListener } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';

type AuthMode = 'signin' | 'signup';
type TopDropdown = 'language' | 'currency';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterOutlet, RouterLink, RouterLinkActive],
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css'],
})
export class AppComponent {
  mobileMenuOpen = false;
  openTopDropdown: TopDropdown | null = null;
  authMode: AuthMode | null = null;
  selectedLanguage = 'English';
  selectedCurrency = 'PKR';
  demoUserName = '';
  authNotice = '';

  signInEmail = '';
  signInPassword = '';

  signUpName = '';
  signUpEmail = '';
  signUpMobile = '';
  signUpPassword = '';

  readonly languages = ['English'];
  readonly currencies = ['PKR'];
  readonly currentYear = new Date().getFullYear();

  @HostListener('document:keydown.escape')
  handleEscape(): void {
    this.closeAuthModal();
    this.openTopDropdown = null;
    this.mobileMenuOpen = false;
  }

  toggleMobileMenu(): void {
    this.mobileMenuOpen = !this.mobileMenuOpen;
    this.openTopDropdown = null;
  }

  closeMobileMenu(): void {
    this.mobileMenuOpen = false;
    this.openTopDropdown = null;
  }

  toggleTopDropdown(menu: TopDropdown): void {
    this.openTopDropdown = this.openTopDropdown === menu ? null : menu;
  }

  chooseLanguage(language: string): void {
    this.selectedLanguage = language;
    this.openTopDropdown = null;
  }

  chooseCurrency(currency: string): void {
    this.selectedCurrency = currency;
    this.openTopDropdown = null;
  }

  openAuthModal(mode: AuthMode): void {
    this.authMode = mode;
    this.authNotice = '';
    this.closeMobileMenu();
  }

  switchAuthMode(mode: AuthMode): void {
    this.authMode = mode;
    this.authNotice = '';
  }

  closeAuthModal(): void {
    this.authMode = null;
    this.authNotice = '';
  }

  submitSignIn(): void {
    if (!this.signInEmail.trim() || this.signInPassword.length < 4) return;

    this.demoUserName = this.signInEmail.split('@')[0] || 'Traveller';
    this.authNotice = 'Demo sign in completed. No account data was sent to the server.';
    this.signInPassword = '';
  }

  submitSignUp(): void {
    if (
      this.signUpName.trim().length < 2 ||
      !this.signUpEmail.trim() ||
      !/^03\d{9}$/.test(this.signUpMobile.trim()) ||
      this.signUpPassword.length < 6
    ) {
      return;
    }

    this.demoUserName = this.signUpName.trim();
    this.authNotice = 'Demo account created locally for this session.';
    this.signUpPassword = '';
  }

  logout(): void {
    this.demoUserName = '';
    this.closeMobileMenu();
  }
}
