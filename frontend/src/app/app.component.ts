import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { AuthService } from './core/services/auth.service';
import { UiOverlayComponent } from './shared/ui.overlay/ui.overlay.component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, UiOverlayComponent ],
  template: `
    <router-outlet></router-outlet>
    <app-ui-overlay></app-ui-overlay>
  `,
})
export class AppComponent {

  constructor(private auth: AuthService) {
    this.auth.init();
  }
}
