import { useState } from 'react';
import Table from '@mui/material/Table';
import TableBody from '@mui/material/TableBody';
import TableCell from '@mui/material/TableCell';
import TableContainer from '@mui/material/TableContainer';
import TableHead from '@mui/material/TableHead';
import TablePagination from '@mui/material/TablePagination';
import TableRow from '@mui/material/TableRow';
import Typography from '@mui/material/Typography';
import IconButton from '@mui/material/IconButton';
import Tooltip from '@mui/material/Tooltip';
import Chip from '@mui/material/Chip';
import CircularProgress from '@mui/material/CircularProgress';
import Box from '@mui/material/Box';
import Stack from '@mui/material/Stack';
import Checkbox from '@mui/material/Checkbox';
import Button from '@mui/material/Button';
import FormControl from '@mui/material/FormControl';
import InputLabel from '@mui/material/InputLabel';
import Select from '@mui/material/Select';
import MenuItem from '@mui/material/MenuItem';
import ToggleButtonGroup from '@mui/material/ToggleButtonGroup';
import ToggleButton from '@mui/material/ToggleButton';
import Alert from '@mui/material/Alert';
import RefreshIcon from '@mui/icons-material/Refresh';
import DeleteOutlineIcon from '@mui/icons-material/DeleteOutline';
import { formatAbsoluteTime, formatRelativeTime } from '../../utils/formatDate';
import type { ArticleSourceType } from '../../api/newsTypes';
import { useFilteredArticles } from './useFilteredArticles';
import { useFilteredArticleProviders } from './useFilteredArticleProviders';
import { useFilteredArticleCategories } from './useFilteredArticleCategories';
import { useDeleteFilteredArticles } from './useDeleteFilteredArticles';
import { DeleteFilteredArticlesDialog } from './DeleteFilteredArticlesDialog';

const PAGE_SIZE_OPTIONS = [10, 25, 50];

export function FilteredArticlesTable() {
  const [page, setPage] = useState(0);
  const [pageSize, setPageSize] = useState(25);
  const [provider, setProvider] = useState<string | null>(null);
  const [sourceType, setSourceType] = useState<ArticleSourceType | null>(null);
  const [category, setCategory] = useState<string | null>(null);
  const [selectedIds, setSelectedIds] = useState<Set<string>>(new Set());
  const [pendingDeleteIds, setPendingDeleteIds] = useState<string[] | null>(null);

  const { data, isLoading, isFetching, isRefetching, refetch } = useFilteredArticles(page, pageSize, {
    provider,
    sourceType,
    category,
  });
  const { data: providers } = useFilteredArticleProviders();
  const { data: categories } = useFilteredArticleCategories();
  const deleteMutation = useDeleteFilteredArticles();

  const articles = data?.items ?? [];

  const allLoadedSelected = articles.length > 0 && articles.every((a) => selectedIds.has(a.id));
  const someLoadedSelected = articles.some((a) => selectedIds.has(a.id));

  const resetToFirstPage = () => {
    setPage(0);
    setSelectedIds(new Set());
  };

  const toggleSelect = (id: string) => {
    setSelectedIds((prev) => {
      const next = new Set(prev);
      if (next.has(id)) {
        next.delete(id);
      } else {
        next.add(id);
      }
      return next;
    });
  };

  const toggleSelectAll = () => {
    setSelectedIds(allLoadedSelected ? new Set() : new Set(articles.map((a) => a.id)));
  };

  const handleConfirmDelete = () => {
    if (!pendingDeleteIds) return;
    deleteMutation.mutate(pendingDeleteIds, {
      onSuccess: () => {
        setSelectedIds((prev) => {
          const next = new Set(prev);
          pendingDeleteIds.forEach((id) => next.delete(id));
          return next;
        });
        setPendingDeleteIds(null);
      },
    });
  };

  return (
    <Box sx={{ position: 'relative' }}>
      <Stack direction="row" alignItems="center" gap={1.5} flexWrap="wrap" sx={{ mb: 1.5 }}>
        <FormControl size="small" sx={{ minWidth: 180 }}>
          <InputLabel id="filtered-articles-provider-filter">Provider</InputLabel>
          <Select
            labelId="filtered-articles-provider-filter"
            label="Provider"
            value={provider ?? ''}
            onChange={(e) => {
              setProvider(e.target.value || null);
              resetToFirstPage();
            }}
          >
            <MenuItem value="">All providers</MenuItem>
            {providers?.map((p) => (
              <MenuItem key={p} value={p}>
                {p}
              </MenuItem>
            ))}
          </Select>
        </FormControl>

        <FormControl size="small" sx={{ minWidth: 180 }}>
          <InputLabel id="filtered-articles-category-filter">Category</InputLabel>
          <Select
            labelId="filtered-articles-category-filter"
            label="Category"
            value={category ?? ''}
            onChange={(e) => {
              setCategory(e.target.value || null);
              resetToFirstPage();
            }}
          >
            <MenuItem value="">All categories</MenuItem>
            {categories?.map((c) => (
              <MenuItem key={c} value={c}>
                {c}
              </MenuItem>
            ))}
          </Select>
        </FormControl>

        <ToggleButtonGroup
          size="small"
          exclusive
          value={sourceType ?? 'All'}
          onChange={(_, value: ArticleSourceType | 'All' | null) => {
            if (!value) return;
            setSourceType(value === 'All' ? null : value);
            resetToFirstPage();
          }}
          aria-label="Feed type"
        >
          <ToggleButton value="All">All</ToggleButton>
          <ToggleButton value="Rss">RSS</ToggleButton>
          <ToggleButton value="Api">API</ToggleButton>
        </ToggleButtonGroup>

        <Tooltip title="Refresh">
          <span>
            <IconButton size="small" aria-label="Refresh" disabled={isRefetching} onClick={() => refetch()}>
              {isRefetching ? <CircularProgress size={18} /> : <RefreshIcon fontSize="small" />}
            </IconButton>
          </span>
        </Tooltip>

        <Tooltip title={allLoadedSelected ? 'Deselect all' : 'Select all rows on this page'}>
          <span>
            <Checkbox
              size="small"
              checked={allLoadedSelected}
              indeterminate={someLoadedSelected && !allLoadedSelected}
              onChange={toggleSelectAll}
              disabled={articles.length === 0}
              aria-label={allLoadedSelected ? 'Deselect all' : 'Select all rows on this page'}
            />
          </span>
        </Tooltip>
      </Stack>

      {selectedIds.size > 0 && (
        <Stack
          direction="row"
          alignItems="center"
          gap={1.5}
          sx={{ px: 1.5, py: 1, mb: 1.5, borderRadius: 1, bgcolor: 'action.selected' }}
        >
          <Typography variant="body2" fontWeight={600}>
            {selectedIds.size} selected
          </Typography>
          <Button size="small" onClick={() => setSelectedIds(new Set())}>
            Clear
          </Button>
          <Button
            size="small"
            color="error"
            variant="outlined"
            startIcon={<DeleteOutlineIcon />}
            sx={{ ml: 'auto' }}
            onClick={() => setPendingDeleteIds([...selectedIds])}
          >
            Delete selected
          </Button>
        </Stack>
      )}

      <TableContainer sx={{ maxHeight: 600, opacity: isFetching && !isLoading ? 0.6 : 1, transition: 'opacity 0.15s' }}>
        <Table size="small" stickyHeader>
          <TableHead>
            <TableRow>
              <TableCell padding="checkbox" />
              <TableCell>Provider</TableCell>
              <TableCell>Title</TableCell>
              <TableCell>Summary</TableCell>
              <TableCell>Category</TableCell>
              <TableCell align="center">Type</TableCell>
              <TableCell>Pulled</TableCell>
              <TableCell align="center">Delete</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {articles.map((article) => (
              <TableRow key={article.id} hover selected={selectedIds.has(article.id)}>
                <TableCell padding="checkbox">
                  <Checkbox
                    size="small"
                    checked={selectedIds.has(article.id)}
                    onChange={() => toggleSelect(article.id)}
                    aria-label={`Select ${article.title}`}
                  />
                </TableCell>
                <TableCell sx={{ whiteSpace: 'nowrap' }}>{article.provider}</TableCell>
                <TableCell sx={{ maxWidth: 320 }}>
                  <Typography variant="body2" noWrap title={article.title}>
                    {article.title}
                  </Typography>
                </TableCell>
                <TableCell sx={{ maxWidth: 420 }}>
                  <Typography variant="body2" color="text.secondary" noWrap title={article.summary ?? ''}>
                    {article.summary || '—'}
                  </Typography>
                </TableCell>
                <TableCell sx={{ whiteSpace: 'nowrap' }}>{article.category}</TableCell>
                <TableCell align="center">
                  <Chip label={article.sourceType} size="small" variant="outlined" />
                </TableCell>
                <TableCell sx={{ whiteSpace: 'nowrap' }}>
                  <Tooltip title={formatAbsoluteTime(article.pulledAt)}>
                    <span>{formatRelativeTime(article.pulledAt)}</span>
                  </Tooltip>
                </TableCell>
                <TableCell align="center">
                  <IconButton size="small" onClick={() => setPendingDeleteIds([article.id])} aria-label="Delete filtered article">
                    <DeleteOutlineIcon fontSize="small" />
                  </IconButton>
                </TableCell>
              </TableRow>
            ))}
            {!isLoading && articles.length === 0 && (
              <TableRow>
                <TableCell colSpan={8} align="center">
                  <Typography variant="body2" color="text.secondary" sx={{ py: 3 }}>
                    {provider || sourceType || category ? 'No filtered articles match this filter.' : 'Nothing has been filtered out yet.'}
                  </Typography>
                </TableCell>
              </TableRow>
            )}
          </TableBody>
        </Table>
      </TableContainer>

      {isLoading && (
        <Box sx={{ display: 'flex', justifyContent: 'center', py: 3 }}>
          <CircularProgress size={24} />
        </Box>
      )}

      <TablePagination
        component="div"
        count={data?.totalCount ?? 0}
        rowsPerPageOptions={PAGE_SIZE_OPTIONS}
        page={page}
        rowsPerPage={pageSize}
        onPageChange={(_, newPage) => {
          setPage(newPage);
          setSelectedIds(new Set());
        }}
        onRowsPerPageChange={(e) => {
          setPageSize(parseInt(e.target.value, 10));
          resetToFirstPage();
        }}
      />

      {deleteMutation.isError && !pendingDeleteIds && (
        <Alert severity="error" sx={{ mt: 1 }}>
          {(deleteMutation.error as Error).message}
        </Alert>
      )}

      <DeleteFilteredArticlesDialog
        open={pendingDeleteIds !== null}
        count={pendingDeleteIds?.length ?? 0}
        submitting={deleteMutation.isPending}
        errorMessage={deleteMutation.isError ? (deleteMutation.error as Error).message : null}
        onCancel={() => setPendingDeleteIds(null)}
        onConfirm={handleConfirmDelete}
      />
    </Box>
  );
}
