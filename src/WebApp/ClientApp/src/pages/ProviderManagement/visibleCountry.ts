/** Temporary: the providers page only lists this country's providers. Set to null to show every country again. */
export const VISIBLE_COUNTRY: string | null = 'India';

export const isVisibleCountry = (country: string) =>
  VISIBLE_COUNTRY === null || country.toLowerCase() === VISIBLE_COUNTRY.toLowerCase();
