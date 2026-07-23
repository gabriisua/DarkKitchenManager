import { HttpParams } from '@angular/common/http';

type Primitive = string | number | boolean | null | undefined;

export function buildPagedParams<T extends object>(
  query: T
): HttpParams {
  let params = new HttpParams();

  for (const [key, value] of Object.entries(query)) {
    // Skip null/undefined/empty
    if (value === null || value === undefined || value === '') continue;

    // Convert camelCase to PascalCase
    const apiKey = key.charAt(0).toUpperCase() + key.slice(1);

    // Handle arrays
    if (Array.isArray(value)) {
      for (const item of value) {
        if (item !== null && item !== undefined) {
          params = params.append(apiKey, String(item));
        }
      }
    } else {
      params = params.set(apiKey, String(value));
    }
  }

  return params;
}
