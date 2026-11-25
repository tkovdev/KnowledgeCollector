import { TestBed } from '@angular/core/testing';
import { App } from './app';

describe('App', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [App],
    }).compileComponents();
  });

  it('should create the app', () => {
    const fixture = TestBed.createComponent(App);
    const app = fixture.componentInstance;
    expect(app).toBeTruthy();
  });

  it('should describe Knowledge Collector and the interview', async () => {
    const fixture = TestBed.createComponent(App);
    await fixture.whenStable();
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.textContent).toContain('Knowledge Collector');
    expect(compiled.querySelector('h1')?.textContent).toContain('Share the knowledge');
    expect(compiled.textContent).toContain('processes and practical knowledge');
    expect(compiled.querySelector('#interview h2')?.textContent).toContain('Interview');
  });

  it('should link to the interview and explain its availability', async () => {
    const fixture = TestBed.createComponent(App);
    await fixture.whenStable();
    const compiled = fixture.nativeElement as HTMLElement;
    const entry = compiled.querySelector<HTMLAnchorElement>('a[href="#interview"]');
    const interview = compiled.querySelector<HTMLElement>('#interview');

    expect(entry?.textContent).toContain('Go to the interview');
    expect(entry?.tabIndex).toBe(0);
    expect(interview?.getAttribute('aria-labelledby')).toBe('interview-heading');
    expect(interview?.querySelector('[role="status"]')?.textContent).toContain(
      'The interview is not available yet',
    );
  });
});
