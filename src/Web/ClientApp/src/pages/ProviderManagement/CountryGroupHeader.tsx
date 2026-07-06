import Box from '@mui/material/Box';
import Stack from '@mui/material/Stack';
import Typography from '@mui/material/Typography';
import Chip from '@mui/material/Chip';
import PublicIcon from '@mui/icons-material/Public';

export function CountryGroupHeader({ country, count }: { country: string; count: number }) {
  return (
    <Stack direction="row" alignItems="center" gap={1} sx={{ mt: 2.5, mb: 1 }}>
      <PublicIcon fontSize="small" sx={{ color: 'text.secondary' }} />
      <Typography variant="subtitle2" fontWeight={700} sx={{ textTransform: 'uppercase', letterSpacing: 0.6 }}>
        {country}
      </Typography>
      <Chip size="small" label={count} sx={{ height: 20, fontWeight: 600 }} />
      <Box sx={{ flexGrow: 1, borderBottom: 1, borderColor: 'divider' }} />
    </Stack>
  );
}
