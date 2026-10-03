import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { from } from 'rxjs';
import { switchMap } from 'rxjs/operators';

import { environment } from '../../../environment';
import { SupabaseService } from '../services/auth-services/supabase.service';

/** Adds the Supabase access token only to requests made to this application's API. */
export const supabaseAuthInterceptor: HttpInterceptorFn = (request, next) => {
  const apiUrl = new URL(environment.apiUrl);
  const requestUrl = new URL(request.url, apiUrl.origin);
  const apiPath = apiUrl.pathname.replace(/\/$/, '');
  const isApiRequest = requestUrl.origin === apiUrl.origin
    && (requestUrl.pathname === apiPath || requestUrl.pathname.startsWith(`${apiPath}/`));

  if (!isApiRequest) {
    return next(request);
  }

  const supabase = inject(SupabaseService);

  return from(supabase.getSession()).pipe(
    switchMap(({ data }) => {
      const accessToken = data.session?.access_token;

      if (!accessToken) {
        return next(request);
      }

      return next(request.clone({
        setHeaders: {
          Authorization: `Bearer ${accessToken}`
        }
      }));
    })
  );
};
