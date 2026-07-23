import { bootstrapApplication } from '@angular/platform-browser';
import { provideRouter, Routes } from '@angular/router';
import { AppComponent } from './app/app.component';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { apiInterceptor} from './app/core/interceptors/api.interceptor';
import { authInterceptor} from './app/core/interceptors/auth.interceptor';
import { routes } from './app/app.routes';


bootstrapApplication(AppComponent, {
  providers: [ provideRouter(routes),
    provideHttpClient(withInterceptors([apiInterceptor, authInterceptor]))

  ]
}).catch(err => console.error(err));
