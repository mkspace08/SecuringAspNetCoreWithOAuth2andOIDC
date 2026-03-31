import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Component, computed, effect, OnInit, signal } from '@angular/core';
import { Image } from './Image';
import { LoggedUser } from './LoggedUser';

@Component({
  selector: 'app-root',
  templateUrl: './app.html',
  standalone: false,
  styleUrl: './app.scss',
})
export class App implements OnInit {
  protected readonly images = signal<Image[]>([]);
  protected readonly loggedUserName = signal<string | undefined>(undefined);
  protected readonly sessionId = signal<string | undefined>(undefined);
  protected readonly isLoggedIn = computed(() => !!this.loggedUserName());

  constructor(private http: HttpClient) {
    // Effect: fetch images when user logs in
    effect(() => {
      if (this.isLoggedIn()) {
        console.log('User logged in, fetching images...');
        this.getImages();
      } else {
        console.log('User not logged in, clearing images...');
        this.images.set([]);
      }
    });
  }

  ngOnInit() {
    console.log('App initialized');
    this.checkLogin();
  }

  getImages() {
    this.http
      .get<Image[]>('/bff/images', { headers: { 'X-CSRF': '1' }, withCredentials: true })
      .subscribe({
        next: (images) => {
          this.images.set(images);
        },
        error: (error: HttpErrorResponse) => {
          this.images.set([]);
          this.handleGetImagesError(error);
        },
      });
  }

  private handleGetImagesError(error: HttpErrorResponse): void {
    if (error.status === 401) {
      this.login();
      return;
    }

    console.error(error);
  }

  login() {
    window.location.href = '/bff/login';
  }

  logout() {
    const sid = this.sessionId();
    window.location.href = sid ? `/bff/logout?sid=${sid}` : '/bff/logout';
  }

  checkLogin() {
    this.http
      .get<LoggedUser>('/bff/logged-user', { headers: { 'X-CSRF': '1' }, withCredentials: true })
      .subscribe({
        next: (user) => {
          console.log('Logged user:', user);
          this.loggedUserName.set(user.name);
          this.sessionId.set(user.sid);
        },
        error: (error: HttpErrorResponse) => {
          this.loggedUserName.set(undefined);
          console.error('Error fetching logged user:', error);
          // isLoggedIn is derived from loggedUserName, so no need to set it directly
          if (error.status === 401) {
            // Możesz tu przekierować do logowania lub wyświetlić przycisk
          }
        },
      });
  }

  protected readonly title = signal('imagegallery.bff.client');
}
