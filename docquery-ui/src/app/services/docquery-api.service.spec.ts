import { TestBed } from '@angular/core/testing';

import { DocqueryApiService } from './docquery-api.service';

describe('DocqueryApiService', () => {
  let service: DocqueryApiService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(DocqueryApiService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
