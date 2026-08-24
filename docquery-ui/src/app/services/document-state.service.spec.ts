import { TestBed } from '@angular/core/testing';

import { DocumentStateService } from './document-state.service';

describe('DocumentStateService', () => {
  let service: DocumentStateService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(DocumentStateService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
